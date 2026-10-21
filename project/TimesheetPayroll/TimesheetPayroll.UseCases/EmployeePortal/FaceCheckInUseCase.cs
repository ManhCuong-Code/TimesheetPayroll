using System.Text.Json;
using TimesheetPayroll.CoreBusiness.Exceptions;
using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.CoreBusiness.Services;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.EmployeePortal;

/// <summary>
/// Yêu cầu chấm công bằng khuôn mặt gửi từ camera trạm.
/// Nhân viên KHÔNG cần chọn mã nhân viên, họ tên, hoặc ca làm việc.
/// Tất cả đều được trích xuất tự động từ mẫu đặc trưng sinh trắc học và lịch phân công.
/// </summary>
public class FacePunchRequest
{
    public float[] Descriptor { get; set; } = Array.Empty<float>();
    public decimal LivenessScore { get; set; } = 0.85m;
    public string? DeviceInfo { get; set; }
    public string PerformedBy { get; set; } = "SYSTEM_KIOSK";
    // Tùy chọn giả lập thử nghiệm danh tính khi môi trường không có webcam
    public long? SimulatedEmployeeId { get; set; }
}

/// <summary>
/// Kết quả phản hồi sau khi nhận diện danh tính và xử lý ca làm việc.
/// </summary>
public class FacePunchResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public Employee? Employee { get; set; }
    public string? PunchType { get; set; } // CHECK_IN hoặc CHECK_OUT
    public TimeSpan? PunchTime { get; set; }
    public WorkShiftStatus? Status { get; set; }
    public decimal? WorkUnits { get; set; }
    public decimal? ActualHours { get; set; }
    public double? Distance { get; set; }
    public double? Confidence { get; set; }
    public string? ShiftName { get; set; }
    public TimeSpan? ShiftStartTime { get; set; }
    public TimeSpan? ShiftEndTime { get; set; }
    public bool IsOvernight { get; set; }
    public DateTime WorkDate { get; set; } = DateTime.Today;
    public string? ResolutionStatus { get; set; }
}

public interface IFaceCheckInUseCase
{
    Task<FacePunchResult> ExecuteAsync(FacePunchRequest request);
}

public class FaceCheckInUseCase : IFaceCheckInUseCase
{
    private readonly IFaceProfileRepository _faceProfileRepo;
    private readonly IFaceAuditLogRepository _faceAuditRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IShiftAssignmentRepository _shiftAssignmentRepo;
    private readonly ITimesheetRepository _timesheetRepo;
    private readonly IWorkShiftRepository _shiftRepo;
    private readonly ITimesheetAuditLogRepository _timesheetAuditRepo;
    private readonly IFaceBiometricService _biometricService;
    private readonly IAttendanceShiftResolver _shiftResolver;

    public FaceCheckInUseCase(
        IFaceProfileRepository faceProfileRepo,
        IFaceAuditLogRepository faceAuditRepo,
        IEmployeeRepository employeeRepo,
        IShiftAssignmentRepository shiftAssignmentRepo,
        ITimesheetRepository timesheetRepo,
        IWorkShiftRepository shiftRepo,
        ITimesheetAuditLogRepository timesheetAuditRepo,
        IFaceBiometricService biometricService,
        IAttendanceShiftResolver shiftResolver)
    {
        _faceProfileRepo = faceProfileRepo;
        _faceAuditRepo = faceAuditRepo;
        _employeeRepo = employeeRepo;
        _shiftAssignmentRepo = shiftAssignmentRepo;
        _timesheetRepo = timesheetRepo;
        _shiftRepo = shiftRepo;
        _timesheetAuditRepo = timesheetAuditRepo;
        _biometricService = biometricService;
        _shiftResolver = shiftResolver;
    }

    public async Task<FacePunchResult> ExecuteAsync(FacePunchRequest request)
    {
        // 1. Kiểm tra tính toàn vẹn của dữ liệu sinh trắc học
        if (request.Descriptor == null || request.Descriptor.Length == 0)
        {
            return new FacePunchResult
            {
                IsSuccess = false,
                Message = "Không nhận được dữ liệu mẫu khuôn mặt từ camera. Vui lòng căn chỉnh thẳng khung hình và thử lại."
            };
        }

        // 2. Kiểm tra tính sống (Anti-Spoofing / Liveness Check)
        if (request.LivenessScore < 0.50m)
        {
            await _faceAuditRepo.LogActionAsync(new FaceAuditLog
            {
                ActionType = "VERIFY_FAIL",
                SimilarityScore = 0,
                PerformedBy = request.PerformedBy,
                DeviceInfo = request.DeviceInfo,
                IsSuccess = false,
                FailureReason = $"Chưa vượt qua kiểm tra tính sống chuyển động (Liveness score = {request.LivenessScore:P0} < 50%)"
            });

            return new FacePunchResult
            {
                IsSuccess = false,
                Message = "Cảnh báo chống giả mạo: Hệ thống chưa phát hiện đủ cử động tính sống tự nhiên (chớp mắt hoặc thở nhẹ). Vui lòng nhìn thẳng và giữ yên."
            };
        }

        // 3. Tìm kiếm hồ sơ khuôn mặt khớp nhất trong danh sách các hồ sơ đang kích hoạt
        var activeProfiles = (await _faceProfileRepo.GetAllActiveProfilesAsync()).ToList();
        if (!activeProfiles.Any())
        {
            return new FacePunchResult
            {
                IsSuccess = false,
                Message = "Hệ thống chưa có hồ sơ khuôn mặt nào được đăng ký hợp lệ bởi Quản trị viên."
            };
        }

        EmployeeFaceProfile? bestMatch = null;
        double distance = 1.0;
        double confidence = 0.0;

        // Nếu có cờ giả lập thử nghiệm (Sandbox Demo không camera)
        if (request.SimulatedEmployeeId.HasValue)
        {
            bestMatch = activeProfiles.FirstOrDefault(p => p.EmployeeId == request.SimulatedEmployeeId.Value);
            distance = 0.12;
            confidence = 96.5;
        }
        else
        {
            var matchResult = _biometricService.FindBestMatch(request.Descriptor, activeProfiles, maxDistanceThreshold: 0.55);
            bestMatch = matchResult.MatchedProfile;
            distance = matchResult.Distance;
            confidence = matchResult.Confidence;
        }

        if (bestMatch == null)
        {
            await _faceAuditRepo.LogActionAsync(new FaceAuditLog
            {
                ActionType = "VERIFY_FAIL",
                SimilarityScore = (decimal)distance,
                PerformedBy = request.PerformedBy,
                DeviceInfo = request.DeviceInfo,
                IsSuccess = false,
                FailureReason = $"Không tìm thấy nhân viên khớp (Khoảng cách tốt nhất = {distance:F3} > ngưỡng 0.55)"
            });

            return new FacePunchResult
            {
                IsSuccess = false,
                Distance = distance,
                Confidence = confidence,
                Message = "Không thể nhận diện danh tính. Khuôn mặt không khớp với hồ sơ nhân viên nào đã đăng ký trong hệ thống."
            };
        }

        // 4. Lấy thông tin nhân viên từ ID sinh trắc học đáng tin cậy
        var employee = await _employeeRepo.GetEmployeeByIdAsync(bestMatch.EmployeeId);
        if (employee == null || employee.Status != "ACTIVE")
        {
            await _faceAuditRepo.LogActionAsync(new FaceAuditLog
            {
                EmployeeId = bestMatch.EmployeeId,
                ActionType = "VERIFY_FAIL",
                SimilarityScore = (decimal)confidence,
                PerformedBy = request.PerformedBy,
                DeviceInfo = request.DeviceInfo,
                IsSuccess = false,
                FailureReason = "Nhân viên không tồn tại hoặc tài khoản không ở trạng thái ACTIVE."
            });

            return new FacePunchResult
            {
                IsSuccess = false,
                Message = "Tài khoản nhân sự này hiện không ở trạng thái làm việc tích cực (ACTIVE)."
            };
        }

        // 5. Tự động xác định ca làm việc từ lịch phân công bằng Stored Procedure SQL Server
        DateTime now = DateTime.Now;
        var resolution = await _shiftAssignmentRepo.ResolveAttendanceShiftViaSpAsync(employee.Id, now);

        if (!resolution.IsSuccess || !resolution.ShiftId.HasValue)
        {
            // Ghi log kiểm toán thất bại phân ca
            await _faceAuditRepo.LogActionAsync(new FaceAuditLog
            {
                EmployeeId = employee.Id,
                ActionType = "SHIFT_RESOLVE_FAIL",
                SimilarityScore = (decimal)confidence,
                PerformedBy = request.PerformedBy,
                DeviceInfo = request.DeviceInfo,
                IsSuccess = false,
                FailureReason = resolution.Message
            });

            return new FacePunchResult
            {
                IsSuccess = false,
                Employee = employee,
                Confidence = confidence,
                Distance = distance,
                ResolutionStatus = resolution.ResolutionStatus,
                Message = resolution.Message
            };
        }

        // 6. Thực hiện ghi nhận chấm công thông qua Stored Procedure giao dịch SQL Server an toàn
        var dbResult = await _shiftAssignmentRepo.ProcessFaceAttendanceViaSpAsync(
            employee.Id,
            resolution.ShiftId.Value,
            resolution.WorkDate,
            now,
            resolution.PunchAction ?? "AUTO",
            request.DeviceInfo,
            request.PerformedBy,
            (decimal)confidence,
            distance
        );

        if (!dbResult.Success)
        {
            return new FacePunchResult
            {
                IsSuccess = false,
                Employee = employee,
                Confidence = confidence,
                Distance = distance,
                ShiftName = dbResult.ShiftName ?? resolution.ShiftName,
                WorkDate = resolution.WorkDate,
                Message = dbResult.Message
            };
        }

        // 7. Chuyển đổi trạng thái sang enum hiển thị
        WorkShiftStatus status = WorkShiftStatus.PRESENT;
        if (!string.IsNullOrWhiteSpace(dbResult.WorkShiftStatus))
        {
            Enum.TryParse(dbResult.WorkShiftStatus, out status);
        }

        return new FacePunchResult
        {
            IsSuccess = true,
            Employee = employee,
            PunchType = dbResult.PunchType,
            PunchTime = dbResult.PunchTime,
            Status = status,
            WorkUnits = dbResult.WorkUnits,
            ActualHours = dbResult.ActualHours,
            ShiftName = dbResult.ShiftName ?? resolution.ShiftName,
            ShiftStartTime = resolution.StartTime,
            ShiftEndTime = resolution.EndTime,
            IsOvernight = resolution.IsOvernight,
            Confidence = confidence,
            Distance = distance,
            WorkDate = resolution.WorkDate,
            ResolutionStatus = resolution.ResolutionStatus,
            Message = dbResult.Message
        };
    }
}
