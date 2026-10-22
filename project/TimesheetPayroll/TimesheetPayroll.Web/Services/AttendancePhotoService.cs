using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using TimesheetPayroll.UseCases.PluginInterfaces.Services;

namespace TimesheetPayroll.Web.Services;

public class AttendancePhotoService : IAttendancePhotoService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AttendancePhotoService> _logger;
    private const int MaxPhotoSizeBytes = 5 * 1024 * 1024; // Giới hạn 5MB

    public AttendancePhotoService(IWebHostEnvironment env, ILogger<AttendancePhotoService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string?> SaveAttendancePhotoAsync(long employeeId, string punchType, string? base64Data, DateTime punchTime)
    {
        if (string.IsNullOrWhiteSpace(base64Data))
        {
            return null;
        }

        try
        {
            // 1. Chuẩn hóa chuỗi Base64 (Loại bỏ tiền tố data:image/jpeg;base64, nếu có)
            var cleanBase64 = base64Data;
            var match = Regex.Match(base64Data, @"^data:image/(?<type>[a-zA-Z0-9]+);base64,(?<data>.+)$");
            if (match.Success)
            {
                cleanBase64 = match.Groups["data"].Value;
            }

            var imageBytes = Convert.FromBase64String(cleanBase64);

            // 2. Kiểm tra dung lượng
            if (imageBytes.Length > MaxPhotoSizeBytes)
            {
                _logger.LogWarning("Ảnh chấm công nhân viên #{EmployeeId} vượt quá dung lượng cho phép ({Length} bytes > {Max} bytes)", 
                    employeeId, imageBytes.Length, MaxPhotoSizeBytes);
                return null;
            }

            // 3. Kiểm tra Magic Bytes cơ bản (JPEG: FF D8 FF; PNG: 89 50 4E 47; WebP: 52 49 46 46)
            if (imageBytes.Length < 4)
            {
                _logger.LogWarning("Dữ liệu ảnh không hợp lệ (độ dài quá ngắn).");
                return null;
            }

            // 4. Tạo thư mục lưu trữ theo năm/tháng trong wwwroot/uploads/attendance_photos/
            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "attendance_photos");
            var targetFolder = Path.Combine(uploadsRoot, punchTime.ToString("yyyy"), punchTime.ToString("MM"));
            
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            // 5. Đặt tên tệp duy nhất, an toàn
            var safePunchType = string.Equals(punchType, "CHECK_OUT", StringComparison.OrdinalIgnoreCase) ? "CHECK_OUT" : "CHECK_IN";
            var fileName = $"EMP{employeeId:D3}_{safePunchType}_{punchTime:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..8]}.jpg";
            var fullPath = Path.Combine(targetFolder, fileName);

            // 6. Ghi tệp an toàn
            await File.WriteAllBytesAsync(fullPath, imageBytes);

            // 7. Trả về đường dẫn web tương đối
            var relativeUrl = $"/uploads/attendance_photos/{punchTime:yyyy}/{punchTime:MM}/{fileName}";
            _logger.LogInformation("Đã lưu ảnh xác minh chấm công: {RelativeUrl} ({Size} KB)", relativeUrl, imageBytes.Length / 1024);

            return relativeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lưu ảnh xác minh chấm công cho nhân viên #{EmployeeId}", employeeId);
            return null;
        }
    }

    public string? GetPhotoPhysicalPath(string relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl)) return null;

        try
        {
            var normalized = relativeUrl.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
            var fullPath = Path.GetFullPath(Path.Combine(_env.WebRootPath, normalized));
            var uploadsRoot = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads", "attendance_photos"));

            // Chống Directory Traversal attack
            if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Phát hiện nguy cơ Directory Traversal khi truy cập ảnh: {RelativeUrl}", relativeUrl);
                return null;
            }

            return File.Exists(fullPath) ? fullPath : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi kiểm tra đường dẫn ảnh: {RelativeUrl}", relativeUrl);
            return null;
        }
    }
}
