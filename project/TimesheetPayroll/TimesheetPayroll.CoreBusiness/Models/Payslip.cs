namespace TimesheetPayroll.CoreBusiness.Models;

public class Payslip
{
    public long Id { get; set; }
    public long PeriodId { get; set; }
    public long EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? DepartmentName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }

    public decimal ActualWorkDays { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public string? Note { get; set; }

    public List<PayslipLine> Lines { get; set; } = new();
}

public class PayslipLine
{
    public long Id { get; set; }
    public long PayslipId { get; set; }
    public PayslipLineType LineType { get; set; }
    public string LineDescription { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsDeduction { get; set; }
}
