using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.CoreBusiness.Services;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.AdminPortal;

public interface IFaceManagementUseCases
{
    Task<IEnumerable<EmployeeFaceProfile>> GetAllProfilesAsync(string? callerRole);
    Task<EmployeeFaceProfile?> GetProfileByEmployeeIdAsync(long employeeId, string? callerRole);
    Task RegisterFaceProfileAsync(long employeeId, float[] descriptor, string? notes, string callerUsername, string? callerRole);
    Task<FaceEnrollmentTelemetryDto> RegisterFaceProfileWithMetricsAsync(
        long employeeId,
        float[] descriptor,
        string? notes,
        string callerUsername,
        string? callerRole,
        FaceEnrollmentTelemetryDto? clientTelemetry = null,
        CancellationToken cancellationToken = default);
    Task ToggleFaceProfileLockAsync(long employeeId, bool isActive, string callerUsername, string? callerRole);
    Task RevokeFaceProfileAsync(long employeeId, string callerUsername, string? callerRole);
    Task UpdateProfileMetadataAsync(long employeeId, string? notes, decimal livenessThreshold, string callerUsername, string? callerRole);
    Task<IEnumerable<FaceAuditLog>> GetAuditLogsAsync(string? callerRole, int limit = 100);
}

public class FaceManagementUseCases : IFaceManagementUseCases
{
    private static readonly ConcurrentDictionary<long, SemaphoreSlim> _enrollmentLocks = new();

    private readonly IFaceProfileRepository _faceProfileRepo;
    private readonly IFaceAuditLogRepository _faceAuditRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IFaceBiometricService _biometricService;

    public FaceManagementUseCases(
        IFaceProfileRepository faceProfileRepo,
        IFaceAuditLogRepository faceAuditRepo,
        IEmployeeRepository employeeRepo,
        IFaceBiometricService? biometricService = null)
    {
        _faceProfileRepo = faceProfileRepo;
        _faceAuditRepo = faceAuditRepo;
        _employeeRepo = employeeRepo;
        _biometricService = biometricService ?? new FaceBiometricService();
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

    public async Task<EmployeeFaceProfile?> GetProfileByEmployeeIdAsync(long employeeId, string? callerRole)
    {
        EnsureAdmin(callerRole);
        return await _faceProfileRepo.GetByEmployeeIdAsync(employeeId);
    }

    public async Task RegisterFaceProfileAsync(long employeeId, float[] descriptor, string? notes, string callerUsername, string? callerRole)
    {
        await RegisterFaceProfileWithMetricsAsync(employeeId, descriptor, notes, callerUsername, callerRole, null, CancellationToken.None);
    }

    public async Task<FaceEnrollmentTelemetryDto> RegisterFaceProfileWithMetricsAsync(
        long employeeId,
        float[] descriptor,
        string? notes,
        string callerUsername,
        string? callerRole,
        FaceEnrollmentTelemetryDto? clientTelemetry = null,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin(callerRole);

        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException("Thao tác đăng ký khuôn mặt đã bị hủy bởi người dùng trước khi xử lý.", cancellationToken);
        }

        var totalSw = Stopwatch.StartNew();
        var telemetry = clientTelemetry ?? new FaceEnrollmentTelemetryDto();
        telemetry.EmployeeId = employeeId;

        var empLock = _enrollmentLocks.GetOrAdd(employeeId, _ => new SemaphoreSlim(1, 1));
        bool lockAcquired = false;
        try
        {
            lockAcquired = await empLock.WaitAsync(TimeSpan.FromSeconds(8), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException("Yêu cầu đăng ký khuôn mặt đã bị hủy trong khi chờ khóa đồng bộ.", cancellationToken);
        }

        if (!lockAcquired)
        {
            throw new InvalidOperationException($"Hệ thống đang xử lý một yêu cầu đăng ký khuôn mặt khác cho nhân viên ID #{employeeId}. Vui lòng không nhấn gửi trùng lặp.");
        }

        try
        {
            // 1. Kiểm tra tính hợp lệ và chuẩn hóa L2 vector 128-D (Embedding Validation)
            var valSw = Stopwatch.StartNew();
            if (!_biometricService.ValidateAndNormalizeDescriptor(descriptor, out var normalizedVector, out var validationError))
            {
                throw new ArgumentException(validationError ?? "Mẫu đặc trưng khuôn mặt 128-D không hợp lệ.", nameof(descriptor));
            }

            // Làm tròn 6 chữ số thập phân để tối ưu kích thước chuỗi JSON lưu trong SQL Server
            var roundedVector = new float[128];
            for (int i = 0; i < 128; i++)
            {
                roundedVector[i] = MathF.Round(normalizedVector[i], 6);
            }
            string descriptorJson = JsonSerializer.Serialize(roundedVector);
            valSw.Stop();
            telemetry.EmbeddingValidationMs = Math.Round(valSw.Elapsed.TotalMilliseconds, 2);

            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException("Thao tác đăng ký khuôn mặt đã bị hủy trước khi ghi vào cơ sở dữ liệu.", cancellationToken);
            }

            // 2. Lưu hồ sơ khuôn mặt + ghi nhật ký kiểm toán trong 1 kết nối & 1 giao dịch SQL nguyên tử
            var profile = new EmployeeFaceProfile
            {
                EmployeeId = employeeId,
                FaceDescriptorJson = descriptorJson,
                RegisteredBy = callerUsername,
                IsActive = true,
                LivenessThreshold = 0.70m,
                Notes = string.IsNullOrWhiteSpace(notes)
                    ? $"Đăng ký bởi Quản trị viên {callerUsername} ngày {DateTime.Now:dd/MM/yyyy HH:mm}"
                    : notes.Trim()
            };

            var auditLog = new FaceAuditLog
            {
                EmployeeId = employeeId,
                ActionType = "REGISTER",
                SimilarityScore = 1.0m,
                PerformedBy = callerUsername,
                DeviceInfo = "Admin Portal Biometric Enrollment",
                IsSuccess = true,
                FailureReason = null
            };

            var dbRes = await _faceProfileRepo.SaveProfileWithAuditAsync(profile, auditLog);
            if (!dbRes.EmployeeExists)
            {
                throw new KeyNotFoundException($"Không tìm thấy nhân viên có mã ID = {employeeId} trong cơ sở dữ liệu.");
            }

            telemetry.SqlQueryMs = dbRes.SqlQueryMs;
            telemetry.DbWriteMs = dbRes.DbWriteMs;
            telemetry.WasAlreadyEnrolled = dbRes.WasAlreadyEnrolled;
            telemetry.SkippedIdenticalWrite = dbRes.SkippedIdenticalWrite;
            telemetry.ReusedExistingEmbedding = dbRes.SkippedIdenticalWrite;

            // Xóa cache vector cũ của nhân viên này để lần nhận diện kế tiếp nạp ngay vector mới
            _biometricService.InvalidateCache(employeeId);

            totalSw.Stop();
            double clientCaptureTotal = telemetry.FaceDetectionMs + telemetry.FaceAlignmentMs + telemetry.PreprocessingMs + telemetry.EmbeddingInferenceMs;
            telemetry.TotalEnrollmentMs = Math.Round(clientCaptureTotal + totalSw.Elapsed.TotalMilliseconds, 2);

            return telemetry;
        }
        finally
        {
            empLock.Release();
        }
    }

    public async Task ToggleFaceProfileLockAsync(long employeeId, bool isActive, string callerUsername, string? callerRole)
    {
        EnsureAdmin(callerRole);

        await _faceProfileRepo.ToggleStatusAsync(employeeId, isActive);
        _biometricService.InvalidateCache(employeeId);

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
        _biometricService.InvalidateCache(employeeId);

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
        _biometricService.InvalidateCache(employeeId);

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
