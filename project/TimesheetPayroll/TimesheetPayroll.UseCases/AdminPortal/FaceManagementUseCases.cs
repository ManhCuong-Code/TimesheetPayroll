using System.Text.Json;
using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.AdminPortal;

public interface IFaceManagementUseCases
{
    Task<IEnumerable<EmployeeFaceProfile>> GetAllProfilesAsync(string? callerRole);
    Task RegisterFaceProfileAsync(long employeeId, float[] descriptor, string? notes, string callerUsername, string? callerRole);
    Task ToggleFaceProfileLockAsync(long employeeId, bool isActive, string callerUsername, string? callerRole);
    Task RevokeFaceProfileAsync(long employeeId, string callerUsername, string? callerRole);
    Task UpdateProfileMetadataAsync(long employeeId, string? notes, decimal livenessThreshold, string callerUsername, string? callerRole);
    Task<IEnumerable<FaceAuditLog>> GetAuditLogsAsync(string? callerRole, int limit = 100);
}

public class FaceManagementUseCases : IFaceManagementUseCases
{
    private readonly IFaceProfileRepository _faceProfileRepo;
    private readonly IFaceAuditLogRepository _faceAuditRepo;
    private readonly IEmployeeRepository _employeeRepo;

    public FaceManagementUseCases(
        IFaceProfileRepository faceProfileRepo,
        IFaceAuditLogRepository faceAuditRepo,
        IEmployeeRepository employeeRepo)
    {
        _faceProfileRepo = faceProfileRepo;
        _faceAuditRepo = faceAuditRepo;
        _employeeRepo = employeeRepo;
    }

    private void EnsureAdmin(string? callerRole)
    {
        if (callerRole != "ROLE_ADMIN")
        {
            throw new UnauthorizedAccessException("TRUY CẬP BỊ TỪ CHỐI (403 Forbidden): Chỉ Quản trị viên cấp cao (ROLE_ADMIN) mới có quyền quản lý và đăng ký dữ liệu sinh trắc học khuôn mặt nhân viên.");
        }
    }

    public async Task<IEnumerable<EmployeeFaceProfile>> GetAllProfilesAsync(string? callerRole)
    {
        EnsureAdmin(callerRole);
        return await _faceProfileRepo.GetAllProfilesWithEmployeeDetailsAsync();
    }

    public async Task RegisterFaceProfileAsync(long employeeId, float[] descriptor, string? notes, string callerUsername, string? callerRole)
    {
        EnsureAdmin(callerRole);

        if (descriptor == null || descriptor.Length < 64)
        {
            throw new ArgumentException("Mẫu đặc trưng khuôn mặt không hợp lệ hoặc không đủ số chiều tiêu chuẩn.", nameof(descriptor));
        }

        var employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy nhân viên có mã ID = {employeeId}.");
        }

        string descriptorJson = JsonSerializer.Serialize(descriptor);

        var profile = new EmployeeFaceProfile
        {
            EmployeeId = employeeId,
            FaceDescriptorJson = descriptorJson,
            RegisteredBy = callerUsername,
            IsActive = true,
            LivenessThreshold = 0.70m,
            Notes = notes ?? $"Đăng ký bởi Quản trị viên {callerUsername} ngày {DateTime.Now:dd/MM/yyyy HH:mm}"
        };

        await _faceProfileRepo.SaveProfileAsync(profile);

        // Ghi log kiểm toán
        await _faceAuditRepo.LogActionAsync(new FaceAuditLog
        {
            EmployeeId = employeeId,
            ActionType = "REGISTER",
            SimilarityScore = 1.0m,
            PerformedBy = callerUsername,
            DeviceInfo = "Admin Portal Biometric Enrollment",
            IsSuccess = true,
            FailureReason = null
        });
    }

    public async Task ToggleFaceProfileLockAsync(long employeeId, bool isActive, string callerUsername, string? callerRole)
    {
        EnsureAdmin(callerRole);

        await _faceProfileRepo.ToggleStatusAsync(employeeId, isActive);

        await _faceAuditRepo.LogActionAsync(new FaceAuditLog
        {
            EmployeeId = employeeId,
            ActionType = isActive ? "UNLOCK" : "LOCK",
            SimilarityScore = null,
            PerformedBy = callerUsername,
            DeviceInfo = "Admin Portal",
            IsSuccess = true,
            FailureReason = null
        });
    }

    public async Task RevokeFaceProfileAsync(long employeeId, string callerUsername, string? callerRole)
    {
        EnsureAdmin(callerRole);

        await _faceProfileRepo.DeleteProfileAsync(employeeId);

        await _faceAuditRepo.LogActionAsync(new FaceAuditLog
        {
            EmployeeId = employeeId,
            ActionType = "REVOKE",
            SimilarityScore = null,
            PerformedBy = callerUsername,
            DeviceInfo = "Admin Portal Biometric Revocation",
            IsSuccess = true,
            FailureReason = null
        });
    }

    public async Task UpdateProfileMetadataAsync(long employeeId, string? notes, decimal livenessThreshold, string callerUsername, string? callerRole)
    {
        EnsureAdmin(callerRole);

        await _faceProfileRepo.UpdateProfileMetadataAsync(employeeId, notes, livenessThreshold);

        await _faceAuditRepo.LogActionAsync(new FaceAuditLog
        {
            EmployeeId = employeeId,
            ActionType = "UPDATE_METADATA",
            SimilarityScore = null,
            PerformedBy = callerUsername,
            DeviceInfo = "Admin Portal Biometric Update",
            IsSuccess = true,
            FailureReason = null
        });
    }

    public async Task<IEnumerable<FaceAuditLog>> GetAuditLogsAsync(string? callerRole, int limit = 100)
    {
        EnsureAdmin(callerRole);
        return await _faceAuditRepo.GetLogsAsync(limit);
    }
}
