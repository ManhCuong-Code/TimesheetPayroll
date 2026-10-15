namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho Phiếu lương chi tiết hàng tháng của từng nhân viên (Aggregate Root).
/// Gồm tổng hợp ngày công, thu nhập gộp, tổng khấu trừ và thực lĩnh chuyển khoản.
/// </summary>
public class Payslip
{
    public long Id { get; set; }
    public long PeriodId { get; set; }
    public string? PeriodCode { get; set; }
    public int PeriodMonth { get; set; }
    public int PeriodYear { get; set; }
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Danh sách các dòng chi tiết cấu thành nên phiếu lương.
    /// </summary>
    public List<PayslipLine> Lines { get; set; } = new();

    /// <summary>
    /// Tính toán lại lương thực lĩnh (Net) từ Gross và TotalDeductions.
    /// </summary>
    public decimal CalculateNetSalary()
    {
        NetSalary = Math.Max(0m, GrossSalary - TotalDeductions);
        return NetSalary;
    }

    /// <summary>
    /// Bổ sung một dòng chi tiết thu nhập vào phiếu lương.
    /// </summary>
    public void AddEarningsLine(PayslipLineType lineType, string description, decimal amount)
    {
        if (amount <= 0) return;

        Lines.Add(new PayslipLine
        {
            PayslipId = Id,
            LineType = lineType,
            LineDescription = description,
            Amount = Math.Round(amount, 0, MidpointRounding.AwayFromZero),
            IsDeduction = false
        });

        RecalculateTotals();
    }

    /// <summary>
    /// Bổ sung một dòng chi tiết khấu trừ vào phiếu lương.
    /// </summary>
    public void AddDeductionLine(PayslipLineType lineType, string description, decimal amount)
    {
        if (amount <= 0) return;

        Lines.Add(new PayslipLine
        {
            PayslipId = Id,
            LineType = lineType,
            LineDescription = description,
            Amount = Math.Round(amount, 0, MidpointRounding.AwayFromZero),
            IsDeduction = true
        });

        RecalculateTotals();
    }

    /// <summary>
    /// Tổng hợp lại toàn bộ Gross, Deductions và Net dựa trên danh sách các dòng chi tiết (Lines).
    /// </summary>
    public void RecalculateTotals()
    {
        GrossSalary = Lines.Where(l => !l.IsDeduction).Sum(l => l.Amount);
        TotalDeductions = Lines.Where(l => l.IsDeduction).Sum(l => l.Amount);
        NetSalary = Math.Max(0m, GrossSalary - TotalDeductions);
        UpdatedAt = DateTime.UtcNow;
    }
}
