using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.CoreBusiness.Services.Interfaces;

/// <summary>
/// Dịch vụ đánh giá và phân loại bảng chấm công hàng ngày (Timesheet Evaluation Service).
/// </summary>
public interface ITimesheetEvaluationService
{
    /// <summary>
    /// Đánh giá bản ghi chấm công từ giờ vào/ra thực tế so với ca làm việc tiêu chuẩn.
    /// </summary>
    void Evaluate(Timesheet timesheet, TimeSpan? standardStartTime = null, TimeSpan? standardEndTime = null);

    /// <summary>
    /// Tính tổng số ngày công hợp lệ trong tháng dựa trên danh sách các bản ghi chấm công.
    /// </summary>
    decimal CalculateTotalWorkDays(IEnumerable<Timesheet> timesheets);

    /// <summary>
    /// Tính tổng số giờ làm thêm (Overtime) trong tháng của nhân viên.
    /// </summary>
    decimal CalculateTotalOvertimeHours(IEnumerable<Timesheet> timesheets, decimal standardDailyHours = 8.0m);
}
