using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.CoreBusiness.Services.Interfaces;

/// <summary>
/// Dịch vụ nghiệp vụ cốt lõi: Tự động phân giải và xác định ca làm việc (Shift Resolution Engine).
/// Truy vấn dữ liệu phân ca thực tế của nhân viên, đối chiếu khung giờ bắt đầu/kết thúc ca,
/// khoảng thời gian cho phép chấm công sớm/muộn, xử lý ca qua đêm và tự động phân biệt Check-In / Check-Out.
/// </summary>
public interface IAttendanceShiftResolver
{
    /// <summary>
    /// Xác định ca làm việc phù hợp và loại chấm công cho nhân viên tại một thời điểm nhất định.
    /// </summary>
    AttendanceShiftResolutionResult ResolveShift(
        long employeeId,
        DateTime punchDateTime,
        IEnumerable<ShiftAssignment> assignedShifts,
        IEnumerable<Timesheet> currentTimesheets);
}
