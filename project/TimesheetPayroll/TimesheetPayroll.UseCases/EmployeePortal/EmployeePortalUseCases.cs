using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.EmployeePortal;

#region 1. TRA CỨU PHIẾU LƯƠNG CÁ NHÂN (UC18 / UC_EMP_04)

/// <summary>
/// Ca sử dụng: Tra cứu chi tiết phiếu lương cá nhân của người lao động (UC18).
/// Áp dụng cơ chế Anti-IDOR: chỉ cho phép người dùng truy vấn phiếu lương thuộc sở hữu của chính mình.
/// </summary>
public interface IViewMyPayslipUseCase
{
    Task<Payslip?> ExecuteAsync(long periodId, long employeeId);
}

public class ViewMyPayslipUseCase : IViewMyPayslipUseCase
{
    private readonly IPayslipRepository _payslipRepo;

    public ViewMyPayslipUseCase(IPayslipRepository payslipRepo)
    {
        _payslipRepo = payslipRepo;
    }

    public async Task<Payslip?> ExecuteAsync(long periodId, long employeeId)
    {
        if (periodId <= 0 || employeeId <= 0)
            return null;

        return await _payslipRepo.GetPayslipByPeriodAndEmployeeAsync(periodId, employeeId);
    }
}

#endregion

#region 2. TRA CỨU BẢNG CHẤM CÔNG CÁ NHÂN (UC10 / UC_EMP_03)

/// <summary>
/// Ca sử dụng: Tra cứu nhật ký chấm công tháng của cá nhân người lao động (UC10).
/// </summary>
public interface IViewMyTimesheetUseCase
{
    Task<IEnumerable<Timesheet>> ExecuteAsync(long employeeId, int month, int year);
}

public class ViewMyTimesheetUseCase : IViewMyTimesheetUseCase
{
    private readonly ITimesheetRepository _timesheetRepo;

    public ViewMyTimesheetUseCase(ITimesheetRepository timesheetRepo)
    {
        _timesheetRepo = timesheetRepo;
    }

    public async Task<IEnumerable<Timesheet>> ExecuteAsync(long employeeId, int month, int year)
    {
        if (employeeId <= 0)
            return Enumerable.Empty<Timesheet>();

        return await _timesheetRepo.GetTimesheetsByEmployeeAndMonthAsync(employeeId, month, year);
    }
}

#endregion

#region 3. NỘP ĐƠN NGHỈ PHÉP TRỰC TUYẾN (UC11 / UC_EMP_02)

/// <summary>
/// Ca sử dụng: Nộp đơn xin nghỉ phép trực tuyến gửi phòng nhân sự (UC11).
/// </summary>
public interface ICreateLeaveRequestUseCase
{
    Task ExecuteAsync(LeaveRequest request);
    Task<IEnumerable<LeaveRequest>> GetMyRequestsAsync(long employeeId);
}

public class CreateLeaveRequestUseCase : ICreateLeaveRequestUseCase
{
    private readonly ILeaveRequestRepository _leaveRepo;

    public CreateLeaveRequestUseCase(ILeaveRequestRepository leaveRepo)
    {
        _leaveRepo = leaveRepo;
    }

    public async Task ExecuteAsync(LeaveRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.EndDate < request.StartDate)
            throw new ArgumentException("Ngày kết thúc nghỉ phép không thể nhỏ hơn ngày bắt đầu.");

        if (request.TotalDays <= 0)
        {
            var span = (request.EndDate.Date - request.StartDate.Date).TotalDays + 1;
            request.TotalDays = Math.Max(1m, (decimal)span);
        }

        request.ApprovalStatus = ApprovalStatus.PENDING;
        await _leaveRepo.CreateLeaveRequestAsync(request);
    }

    public async Task<IEnumerable<LeaveRequest>> GetMyRequestsAsync(long employeeId)
    {
        return await _leaveRepo.GetLeaveRequestsByEmployeeAsync(employeeId);
    }
}

#endregion

#region 4. ĐIỂM DANH CHECK-IN / CHECK-OUT HÀNG NGÀY (UC09 / UC_EMP_01)

/// <summary>
/// Ca sử dụng: Điểm danh vào ca (Check-In) và tan ca (Check-Out) hàng ngày (UC09).
/// Tự động ghi nhận mốc thời gian hệ thống và tính toán tổng giờ làm việc thực tế.
/// </summary>
public interface ICheckInCheckOutUseCase
{
    Task<Timesheet> ExecuteCheckInAsync(long employeeId, long? shiftId = null, TimeSpan? customTime = null, DateTime? workDate = null);
    Task<Timesheet> ExecuteCheckOutAsync(long employeeId, long? shiftId = null, TimeSpan? customTime = null, DateTime? workDate = null);
    Task<Timesheet> ExecuteManualPunchAsync(long employeeId, long? shiftId, DateTime workDate, TimeSpan? checkInTime, TimeSpan? checkOutTime, string? customNote = null);
    Task<Timesheet> ChangeShiftAsync(long employeeId, long newShiftId, DateTime workDate);
}

public class CheckInCheckOutUseCase : ICheckInCheckOutUseCase
{
    private readonly ITimesheetRepository _timesheetRepo;
    private readonly IWorkShiftRepository _shiftRepo;

    public CheckInCheckOutUseCase(
        ITimesheetRepository timesheetRepo,
        IWorkShiftRepository shiftRepo)
    {
        _timesheetRepo = timesheetRepo;
        _shiftRepo = shiftRepo;
    }

    public async Task<Timesheet> ExecuteCheckInAsync(long employeeId, long? shiftId = null, TimeSpan? customTime = null, DateTime? workDate = null)
    {
        if (employeeId <= 0)
            throw new ArgumentException("Mã nhân viên không hợp lệ.", nameof(employeeId));

        var targetDate = (workDate ?? DateTime.Today).Date;
        var existing = (await _timesheetRepo.GetTimesheetsByEmployeeAndMonthAsync(employeeId, targetDate.Month, targetDate.Year))
            .FirstOrDefault(t => t.WorkDate.Date == targetDate);

        if (existing != null && existing.IsLocked)
            throw new InvalidOperationException("Bản ghi chấm công của ngày này đã bị khóa sổ và không thể điểm danh.");

        // Xác định ca làm việc
        WorkShift? shift = null;
        if (shiftId.HasValue && shiftId.Value > 0)
        {
            shift = await _shiftRepo.GetWorkShiftByIdAsync(shiftId.Value);
        }
        if (shift == null)
        {
            var allShifts = await _shiftRepo.GetWorkShiftsAsync();
            shift = allShifts.FirstOrDefault(s => s.IsActive && s.ShiftCode == "CA_HANH_CHINH") 
                 ?? allShifts.FirstOrDefault(s => s.IsActive);
        }

        var timesheet = existing ?? new Timesheet
        {
            EmployeeId = employeeId,
            WorkDate = targetDate,
            WorkUnits = shift?.WorkUnits ?? 1.0m,
            WorkShiftStatus = WorkShiftStatus.PRESENT
        };

        timesheet.CheckInTime = customTime ?? DateTime.Now.TimeOfDay;
        timesheet.EvaluateCheckIn(shift);

        await _timesheetRepo.AddOrUpdateTimesheetAsync(timesheet);
        return timesheet;
    }

    public async Task<Timesheet> ExecuteCheckOutAsync(long employeeId, long? shiftId = null, TimeSpan? customTime = null, DateTime? workDate = null)
    {
        if (employeeId <= 0)
            throw new ArgumentException("Mã nhân viên không hợp lệ.", nameof(employeeId));

        var targetDate = (workDate ?? DateTime.Today).Date;
        var existing = (await _timesheetRepo.GetTimesheetsByEmployeeAndMonthAsync(employeeId, targetDate.Month, targetDate.Year))
            .FirstOrDefault(t => t.WorkDate.Date == targetDate);

        if (existing != null && existing.IsLocked)
            throw new InvalidOperationException("Bản ghi chấm công của ngày này đã bị khóa sổ và không thể điểm danh.");

        // Xác định ca
        WorkShift? shift = null;
        if (shiftId.HasValue && shiftId.Value > 0)
        {
            shift = await _shiftRepo.GetWorkShiftByIdAsync(shiftId.Value);
        }
        else if (existing?.ShiftId.HasValue == true)
        {
            shift = await _shiftRepo.GetWorkShiftByIdAsync(existing.ShiftId.Value);
        }
        if (shift == null)
        {
            var allShifts = await _shiftRepo.GetWorkShiftsAsync();
            shift = allShifts.FirstOrDefault(s => s.IsActive && s.ShiftCode == "CA_HANH_CHINH") 
                 ?? allShifts.FirstOrDefault(s => s.IsActive);
        }

        var timesheet = existing ?? new Timesheet
        {
            EmployeeId = employeeId,
            WorkDate = targetDate,
            WorkUnits = shift?.WorkUnits ?? 1.0m,
            WorkShiftStatus = WorkShiftStatus.PRESENT,
            CheckInTime = shift?.StartTime ?? new TimeSpan(8, 0, 0)
        };

        timesheet.CheckOutTime = customTime ?? DateTime.Now.TimeOfDay;
        timesheet.EvaluateShift(shift);

        await _timesheetRepo.AddOrUpdateTimesheetAsync(timesheet);
        return timesheet;
    }

    public async Task<Timesheet> ExecuteManualPunchAsync(long employeeId, long? shiftId, DateTime workDate, TimeSpan? checkInTime, TimeSpan? checkOutTime, string? customNote = null)
    {
        if (employeeId <= 0)
            throw new ArgumentException("Mã nhân viên không hợp lệ.", nameof(employeeId));

        if (!checkInTime.HasValue && !checkOutTime.HasValue)
            throw new ArgumentException("Vui lòng nhập giờ vào (Check-In) hoặc giờ ra (Check-Out).");

        var targetDate = workDate.Date;
        var existing = (await _timesheetRepo.GetTimesheetsByEmployeeAndMonthAsync(employeeId, targetDate.Month, targetDate.Year))
            .FirstOrDefault(t => t.WorkDate.Date == targetDate);

        if (existing != null && existing.IsLocked)
            throw new InvalidOperationException("Bản ghi chấm công của ngày này đã bị khóa sổ và không thể chỉnh sửa.");

        WorkShift? shift = null;
        if (shiftId.HasValue && shiftId.Value > 0)
        {
            shift = await _shiftRepo.GetWorkShiftByIdAsync(shiftId.Value);
        }
        else if (existing?.ShiftId.HasValue == true)
        {
            shift = await _shiftRepo.GetWorkShiftByIdAsync(existing.ShiftId.Value);
        }
        if (shift == null)
        {
            var allShifts = await _shiftRepo.GetWorkShiftsAsync();
            shift = allShifts.FirstOrDefault(s => s.IsActive && s.ShiftCode == "CA_HANH_CHINH") 
                 ?? allShifts.FirstOrDefault(s => s.IsActive);
        }

        var timesheet = existing ?? new Timesheet
        {
            EmployeeId = employeeId,
            WorkDate = targetDate,
            WorkUnits = shift?.WorkUnits ?? 1.0m,
            WorkShiftStatus = WorkShiftStatus.PRESENT
        };

        if (shift != null)
        {
            timesheet.ShiftId = shift.Id;
            timesheet.ShiftCode = shift.ShiftCode;
            timesheet.ShiftName = shift.ShiftName;
        }

        if (checkInTime.HasValue)
        {
            timesheet.CheckInTime = checkInTime.Value;
        }
        else if (!timesheet.CheckInTime.HasValue)
        {
            timesheet.CheckInTime = shift?.StartTime ?? new TimeSpan(8, 0, 0);
        }

        if (checkOutTime.HasValue)
        {
            timesheet.CheckOutTime = checkOutTime.Value;
            timesheet.EvaluateShift(shift);
        }
        else
        {
            timesheet.EvaluateCheckIn(shift);
        }

        if (!string.IsNullOrWhiteSpace(customNote))
        {
            timesheet.Note = string.IsNullOrWhiteSpace(timesheet.Note) 
                ? customNote 
                : $"{timesheet.Note} • {customNote}";
        }

        await _timesheetRepo.AddOrUpdateTimesheetAsync(timesheet);
        return timesheet;
    }

    public async Task<Timesheet> ChangeShiftAsync(long employeeId, long newShiftId, DateTime workDate)
    {
        if (employeeId <= 0)
            throw new ArgumentException("Mã nhân viên không hợp lệ.", nameof(employeeId));
        if (newShiftId <= 0)
            throw new ArgumentException("Mã ca làm việc không hợp lệ.", nameof(newShiftId));

        var shift = await _shiftRepo.GetWorkShiftByIdAsync(newShiftId);
        if (shift == null)
            throw new InvalidOperationException($"Không tìm thấy ca làm việc với ID = {newShiftId}.");

        var targetDate = workDate.Date;
        var existing = (await _timesheetRepo.GetTimesheetsByEmployeeAndMonthAsync(employeeId, targetDate.Month, targetDate.Year))
            .FirstOrDefault(t => t.WorkDate.Date == targetDate);

        if (existing != null && existing.IsLocked)
            throw new InvalidOperationException("Bản ghi chấm công của ngày này đã bị khóa sổ và không thể thay đổi ca.");

        var timesheet = existing ?? new Timesheet
        {
            EmployeeId = employeeId,
            WorkDate = targetDate,
            WorkUnits = shift.WorkUnits,
            WorkShiftStatus = WorkShiftStatus.PRESENT,
            Note = $"Đăng ký ca làm việc: {shift.ShiftName}"
        };

        timesheet.ShiftId = shift.Id;
        timesheet.ShiftCode = shift.ShiftCode;
        timesheet.ShiftName = shift.ShiftName;
        timesheet.WorkUnits = shift.WorkUnits;

        if (timesheet.CheckOutTime.HasValue)
        {
            timesheet.EvaluateShift(shift);
        }
        else if (timesheet.CheckInTime.HasValue)
        {
            timesheet.EvaluateCheckIn(shift);
        }
        else
        {
            timesheet.Note = $"Đã đăng ký ca: {shift.ShiftName}";
        }

        await _timesheetRepo.AddOrUpdateTimesheetAsync(timesheet);
        return timesheet;
    }
}

#endregion

#region 5. LẤY HỒ SƠ NHÂN VIÊN HIỆN TẠI (UC_EMP_PROFILE)

public interface IGetEmployeeProfileUseCase
{
    Task<Employee?> ExecuteAsync(long employeeId);
}

public class GetEmployeeProfileUseCase : IGetEmployeeProfileUseCase
{
    private readonly IEmployeeRepository _employeeRepo;

    public GetEmployeeProfileUseCase(IEmployeeRepository employeeRepo)
    {
        _employeeRepo = employeeRepo;
    }

    public async Task<Employee?> ExecuteAsync(long employeeId)
    {
        if (employeeId <= 0) return null;
        return await _employeeRepo.GetEmployeeByIdAsync(employeeId);
    }
}

#endregion

#region 6. GỬI YÊU CẦU CHẤM CÔNG THỦ CÔNG (UC_EMP_MANUAL_ATTENDANCE)

/// <summary>
/// Ca sử dụng: Nhân viên nộp yêu cầu chấm công bổ sung / điều chỉnh giờ làm việc (quên chấm công, lỗi thiết bị).
/// </summary>
public interface ICreateAttendanceRequestUseCase
{
    Task<long> SubmitRequestAsync(AttendanceRequest request);
    Task<IEnumerable<AttendanceRequest>> GetMyRequestsAsync(long employeeId);
}

public class CreateAttendanceRequestUseCase : ICreateAttendanceRequestUseCase
{
    private readonly IAttendanceRequestRepository _attendanceRequestRepo;

    public CreateAttendanceRequestUseCase(IAttendanceRequestRepository attendanceRequestRepo)
    {
        _attendanceRequestRepo = attendanceRequestRepo;
    }

    public async Task<long> SubmitRequestAsync(AttendanceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.EmployeeId <= 0)
            throw new ArgumentException("Mã nhân viên không hợp lệ.", nameof(request.EmployeeId));

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new ArgumentException("Vui lòng ghi rõ lý do yêu cầu chấm công thủ công.", nameof(request.Reason));

        if (request.WorkDate.Date > DateTime.Today)
            throw new ArgumentException("Không thể gửi yêu cầu chấm công bổ sung cho ngày trong tương lai.", nameof(request.WorkDate));

        if (request.RequestType == "BOTH" && request.ProposedCheckIn.HasValue && request.ProposedCheckOut.HasValue)
        {
            if (request.ProposedCheckOut.Value <= request.ProposedCheckIn.Value)
                throw new ArgumentException("Giờ ra đề nghị phải sau giờ vào đề nghị.");
        }

        request.ApprovalStatus = "PENDING";
        return await _attendanceRequestRepo.CreateAttendanceRequestAsync(request);
    }

    public async Task<IEnumerable<AttendanceRequest>> GetMyRequestsAsync(long employeeId)
    {
        if (employeeId <= 0)
            return Enumerable.Empty<AttendanceRequest>();

        return await _attendanceRequestRepo.GetAttendanceRequestsByEmployeeAsync(employeeId);
    }
}

#endregion

