namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho dòng tổng hợp chấm công của một nhân viên trong kỳ/tháng phục vụ đối soát và xuất báo cáo.
/// </summary>
public class EmployeeTimesheetSummaryItem
{
    public int OrderNumber { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? Position { get; set; }
    public string? PositionName { get => Position; set => Position = value; }
    public decimal TotalWorkUnits { get; set; }
    public decimal TotalHours { get; set; }
    public int Standard8hCount { get; set; }
    public int SplitShiftCount { get; set; }
    public int FullShiftCount { get; set; }
    public int HalfShiftCount { get; set; }
    public int LateOrEarlyCount { get; set; }
    public int PaidLeaveCount { get; set; }
    public int AbsentCount { get; set; }
    public decimal OvertimeHours { get; set; }
    public DateTime? LastWorkDate { get; set; }
    public string Status { get; set; } = "ĐÃ ĐỐI SOÁT";
    public string ConfirmationStatus { get => Status; set => Status = value; }
    public string? Note { get; set; }
}
