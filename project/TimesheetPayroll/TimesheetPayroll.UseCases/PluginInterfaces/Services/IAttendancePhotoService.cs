namespace TimesheetPayroll.UseCases.PluginInterfaces.Services;

/// <summary>
/// Giao diện xử lý lưu trữ và xác thực ảnh chụp xác minh khi chấm công.
/// </summary>
public interface IAttendancePhotoService
{
    /// <summary>
    /// Lưu ảnh chụp từ chuỗi Base64 (JPEG/PNG) vào thư mục lưu trữ máy chủ an toàn.
    /// Trả về đường dẫn URL tương đối (ví dụ /uploads/attendance_photos/2026/10/EMP001_CHECK_IN_...jpg).
    /// </summary>
    Task<string?> SaveAttendancePhotoAsync(long employeeId, string punchType, string? base64Data, DateTime punchTime);

    /// <summary>
    /// Lấy đường dẫn vật lý an toàn của tệp ảnh trên máy chủ (chống Directory Traversal).
    /// </summary>
    string? GetPhotoPhysicalPath(string relativeUrl);
}
