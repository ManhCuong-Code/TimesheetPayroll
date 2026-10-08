namespace TimesheetPayroll.CoreBusiness.Models;

public class Department
{
    public long Id { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public string? Description { get; set; }
}
