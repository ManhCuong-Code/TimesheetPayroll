namespace TimesheetPayroll.CoreBusiness.Models;

public class Timesheet
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public long? PeriodId { get; set; }
    public DateTime WorkDate { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public decimal ActualHoursWorked { get; set; } = 8.0m;
    public WorkShiftStatus WorkShiftStatus { get; set; } = WorkShiftStatus.PRESENT;
    public decimal WorkUnits { get; set; } = 1.0m;
    public bool IsLocked { get; set; } = false;
    public string? Note { get; set; }
}
