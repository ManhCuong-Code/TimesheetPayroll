namespace TimesheetPayroll.CoreBusiness.Models;

public class Employee
{
    public long Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string IdentityCardNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public long? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime HireDate { get; set; } = DateTime.Today;
    public string Status { get; set; } = "ACTIVE";

    // Hợp đồng & Lương cơ bản liên kết
    public decimal BaseSalary { get; set; } = 10000000m;
    public decimal MealAllowance { get; set; } = 500000m;
    public decimal FuelAllowance { get; set; } = 0m;
}
