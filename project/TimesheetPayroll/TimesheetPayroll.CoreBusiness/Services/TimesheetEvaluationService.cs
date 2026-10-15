using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;

namespace TimesheetPayroll.CoreBusiness.Services;

/// <summary>
/// Triển khai dịch vụ đánh giá chấm công và tổng hợp ngày công / giờ làm thêm hàng tháng.
/// </summary>
public class TimesheetEvaluationService : ITimesheetEvaluationService
{
    public void Evaluate(Timesheet timesheet, TimeSpan? standardStartTime = null, TimeSpan? standardEndTime = null)
    {
        ArgumentNullException.ThrowIfNull(timesheet);
        timesheet.EvaluateShift(standardStartTime, standardEndTime);
    }

    public decimal CalculateTotalWorkDays(IEnumerable<Timesheet> timesheets)
    {
        if (timesheets == null) return 0m;
        return timesheets.Sum(t => t.WorkUnits);
    }

    public decimal CalculateTotalOvertimeHours(IEnumerable<Timesheet> timesheets, decimal standardDailyHours = 8.0m)
    {
        if (timesheets == null) return 0m;

        decimal totalOt = 0m;
        foreach (var t in timesheets)
        {
            if (t.ActualHoursWorked > standardDailyHours)
            {
                totalOt += (t.ActualHoursWorked - standardDailyHours);
            }
        }

        return Math.Round(totalOt, 1, MidpointRounding.AwayFromZero);
    }
}
