using System.Text.Json;
using TimesheetPayroll.CoreBusiness.Exceptions;
using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.CoreBusiness.Services;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.EmployeePortal;

public class FacePunchRequest
{
    public float[] Descriptor { get; set; } = Array.Empty<float>();
    public decimal LivenessScore { get; set; } = 0.85m;
    public string? PreferredPunchType { get; set; } = "AUTO"; // AUTO, CHECK_IN, CHECK_OUT
    public long? ShiftId { get; set; }
    public string? DeviceInfo { get; set; }
    public string PerformedBy { get; set; } = "SYSTEM_KIOSK";
}

public class FacePunchResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public Employee? Employee { get; set; }
    public string? PunchType { get; set; } // CHECK_IN hoặc CHECK_OUT
    public TimeSpan? PunchTime { get; set; }
    public WorkShiftStatus? Status { get; set; }
    public decimal? WorkUnits { get; set; }
    public double? Distance { get; set; }
    public double? Confidence { get; set; }
    public string? ShiftName { get; set; }
    public DateTime WorkDate { get; set; } = DateTime.Today;
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
    private readonly ITimesheetRepository _timesheetRepo;
    private readonly IWorkShiftRepository _shiftRepo;
    private readonly ITimesheetAuditLogRepository _timesheetAuditRepo;
    private readonly IFaceBiometricService _biometricService;

    public FaceCheckInUseCase(
        IFaceProfileRepository faceProfileRepo,
        IFaceAuditLogRepository faceAuditRepo,
        IEmployeeRepository employeeRepo,
        ITimesheetRepository timesheetRepo,
        IWorkShiftRepository shiftRepo,
        ITimesheetAuditLogRepository timesheetAuditRepo,
        IFaceBiometricService biometricService)
    {
        _faceProfileRepo = faceProfileRepo;
        _faceAuditRepo = faceAuditRepo;
        _employeeRepo = employeeRepo;
        _timesheetRepo = timesheetRepo;
        _shiftRepo = shiftRepo;
        _timesheetAuditRepo = timesheetAuditRepo;
        _biometricService = biometricService;
    }

    public async Task<FacePunchResult> ExecuteAsync(FacePunchRequest request)
    {
        if (request.Descriptor == null || request.Descriptor.Length == 0)
        {
            return new FacePunchResult
            {
                IsSuccess = false,
                Message = "Không nhận được dữ liệu mẫu khuôn mặt từ camera. Vui lòng thử lại."
            };
        }

        // 1. Kiểm tra tính sống (Anti-spoofing / Liveness Check)
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
                Message = "Cảnh báo chống giả mạo: Hệ thống chưa phát hiện đủ cử động tính sống tự nhiên (chớp mắt hoặc chuyển động đầu). Vui lòng đứng trước camera và chớp mắt nhẹ."
            };
        }

        // 2. Tìm kiếm khuôn mặt khớp nhất trong danh sách các hồ sơ đang kích hoạt
        var activeProfiles = (await _faceProfileRepo.GetAllActiveProfilesAsync()).ToList();
        if (!activeProfiles.Any())
        {
            return new FacePunchResult
            {
                IsSuccess = false,
                Message = "Hệ thống chưa có hồ sơ khuôn mặt nào được đăng ký hợp lệ bởi Quản trị viên."
            };
        }

        var (bestMatch, distance, confidence) = _biometricService.FindBestMatch(request.Descriptor, activeProfiles, maxDistanceThreshold: 0.55);

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

        // 3. Lấy thông tin nhân viên
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
                FailureReason = "Nhân viên không tồn tại hoặc đã tạm dừng công tác / nghỉ việc."
            });

            return new FacePunchResult
            {
                IsSuccess = false,
                Message = "Tài khoản nhân sự này hiện không ở trạng thái làm việc tích cực (ACTIVE)."
            };
        }

        // 4. Lấy ca làm việc
        WorkShift? shift = null;
        if (request.ShiftId.HasValue)
        {
            shift = await _shiftRepo.GetWorkShiftByIdAsync(request.ShiftId.Value);
        }
        if (shift == null)
        {
            shift = await _shiftRepo.GetWorkShiftByCodeAsync("CA_HANH_CHINH") 
                 ?? (await _shiftRepo.GetWorkShiftsAsync()).FirstOrDefault();
        }

        // 5. Kiểm tra bảng chấm công hôm nay
        var today = DateTime.Today;
        var monthTimesheets = (await _timesheetRepo.GetTimesheetsByEmployeeAndMonthAsync(employee.Id, today.Month, today.Year)).ToList();
        var todayTimesheet = monthTimesheets.FirstOrDefault(t => t.WorkDate.Date == today);

        var nowTime = DateTime.Now.TimeOfDay;

        // 6. Kiểm tra chống chấm công trùng lặp (Cooldown 3 phút)
        if (todayTimesheet != null)
        {
            TimeSpan? lastPunch = todayTimesheet.CheckOutTime ?? todayTimesheet.CheckInTime;
            if (lastPunch.HasValue)
            {
                var diff = Math.Abs((nowTime - lastPunch.Value).TotalMinutes);
                if (diff < 3.0)
                {
                    await _faceAuditRepo.LogActionAsync(new FaceAuditLog
                    {
                        EmployeeId = employee.Id,
                        ActionType = "PUNCH_REJECT_COOLDOWN",
                        SimilarityScore = (decimal)confidence,
                        PerformedBy = request.PerformedBy,
                        DeviceInfo = request.DeviceInfo,
                        IsSuccess = false,
                        FailureReason = $"Thao tác trùng lặp trong khoảng thời gian {diff:F1} phút"
                    });

                    return new FacePunchResult
                    {
                        IsSuccess = false,
                        Employee = employee,
                        Confidence = confidence,
                        Distance = distance,
                        Message = $"Bạn vừa chấm công lúc {lastPunch.Value:hh\\:mm\\:ss} ({Math.Round(diff, 1)} phút trước). Vui lòng không quét liên tục để tránh dữ liệu trùng lặp."
                    };
                }
            }
        }

        // 7. Xác định loại chấm công (CHECK_IN hay CHECK_OUT)
        string punchAction;
        string beforeJson = todayTimesheet != null ? JsonSerializer.Serialize(todayTimesheet) : "null";

        if (todayTimesheet == null)
        {
            // Lần đầu trong ngày => Check-In
            todayTimesheet = new Timesheet
            {
                EmployeeId = employee.Id,
                WorkDate = today,
                ShiftId = shift?.Id,
                ShiftCode = shift?.ShiftCode,
                ShiftName = shift?.ShiftName,
                CheckInTime = nowTime,
                IsLocked = false
            };
            todayTimesheet.EvaluateCheckIn(shift);
            punchAction = "CHECK_IN";
        }
        else if (!todayTimesheet.CheckInTime.HasValue)
        {
            todayTimesheet.ShiftId = shift?.Id ?? todayTimesheet.ShiftId;
            todayTimesheet.ShiftCode = shift?.ShiftCode ?? todayTimesheet.ShiftCode;
            todayTimesheet.ShiftName = shift?.ShiftName ?? todayTimesheet.ShiftName;
            todayTimesheet.CheckInTime = nowTime;
            todayTimesheet.EvaluateCheckIn(shift);
            punchAction = "CHECK_IN";
        }
        else
        {
            // Đã có Check-In => Ghi nhận Check-Out (Hết ca)
            todayTimesheet.CheckOutTime = nowTime;
            todayTimesheet.EvaluateShift(shift);
            punchAction = "CHECK_OUT";
        }

        // 8. Lưu vào cơ sở dữ liệu
        await _timesheetRepo.AddOrUpdateTimesheetAsync(todayTimesheet);

        // 9. Ghi log kiểm toán khuôn mặt
        await _faceAuditRepo.LogActionAsync(new FaceAuditLog
        {
            EmployeeId = employee.Id,
            ActionType = punchAction == "CHECK_IN" ? "PUNCH_CHECK_IN" : "PUNCH_CHECK_OUT",
            SimilarityScore = (decimal)confidence,
            PerformedBy = request.PerformedBy,
            DeviceInfo = request.DeviceInfo,
            IsSuccess = true,
            FailureReason = null
        });

        // 10. Ghi log lịch sử chấm công
        string afterJson = JsonSerializer.Serialize(todayTimesheet);
        await _timesheetAuditRepo.LogAuditAsync(new TimesheetAuditLog
        {
            TimesheetId = todayTimesheet.Id,
            EmployeeId = employee.Id,
            Action = punchAction == "CHECK_IN" ? "FACE_CHECK_IN" : "FACE_CHECK_OUT",
            BeforeData = beforeJson,
            AfterData = afterJson,
            ModifiedBy = $"{employee.FullName} (Camera Face ID)",
            Reason = $"Xác thực khuôn mặt tự động (Độ tin cậy: {confidence:F1}%, Khoảng cách: {distance:F3})"
        });

        string punchDisplay = punchAction == "CHECK_IN" ? "VÀO CA (Check-In)" : "HẾT CA (Check-Out)";
        string timeDisplay = DateTime.Today.Add(nowTime).ToString("HH:mm:ss");

        return new FacePunchResult
        {
            IsSuccess = true,
            Employee = employee,
            PunchType = punchAction,
            PunchTime = nowTime,
            Status = todayTimesheet.WorkShiftStatus,
            WorkUnits = todayTimesheet.WorkUnits,
            ShiftName = todayTimesheet.ShiftName ?? shift?.ShiftName,
            Confidence = confidence,
            Distance = distance,
            WorkDate = today,
            Message = $"Ghi nhận {punchDisplay} thành công cho nhân viên [{employee.EmployeeCode}] {employee.FullName} lúc {timeDisplay}."
        };
    }
}
