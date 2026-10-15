namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Cấu hình tỷ lệ trích nộp bảo hiểm bắt buộc theo luật định (BHXH, BHYT, BHTN).
/// Cho phép cập nhật động theo chính sách mà không cần sửa đổi mã nguồn.
/// </summary>
public class DeductionRate
{
    public long Id { get; set; }
    public string RateCode { get; set; } = string.Empty;
    public string RateName { get; set; } = string.Empty;
    public decimal EmployeeRate { get; set; } // Ví dụ: 0.08m (8%)
    public decimal EmployerRate { get; set; } // Ví dụ: 0.175m (17.5%)
    public DateTime EffectiveFrom { get; set; } = DateTime.Today;
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tính số tiền trích nộp phần người lao động đóng dựa trên lương cơ bản đóng bảo hiểm,
    /// áp dụng mức trần theo Nghị định 73/2024/NĐ-CP (BHXH, BHYT tối đa 46.8tr) và Nghị định 74/2024/NĐ-CP (BHTN tối đa 99.2tr).
    /// </summary>
    public decimal CalculateEmployeeDeduction(decimal baseSalary)
    {
        if (!IsActive || baseSalary <= 0 || EmployeeRate <= 0) return 0m;

        decimal insurableSalary = GetInsurableSalaryCap(baseSalary);
        return Math.Round(insurableSalary * EmployeeRate, 0, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Tính số tiền trích nộp phần người sử dụng lao động (doanh nghiệp) đóng,
    /// áp dụng mức trần theo Nghị định 73/2024/NĐ-CP và Nghị định 74/2024/NĐ-CP.
    /// </summary>
    public decimal CalculateEmployerDeduction(decimal baseSalary)
    {
        if (!IsActive || baseSalary <= 0 || EmployerRate <= 0) return 0m;

        decimal insurableSalary = GetInsurableSalaryCap(baseSalary);
        return Math.Round(insurableSalary * EmployerRate, 0, MidpointRounding.AwayFromZero);
    }

    private decimal GetInsurableSalaryCap(decimal baseSalary)
    {
        if (RateCode.Equals("BHXH", StringComparison.OrdinalIgnoreCase) || 
            RateCode.Equals("BHYT", StringComparison.OrdinalIgnoreCase))
        {
            return Math.Min(baseSalary, VietnameseLaborRegulations.MaxSalaryCapBhxhBhyt);
        }
        else if (RateCode.Equals("BHTN", StringComparison.OrdinalIgnoreCase))
        {
            return Math.Min(baseSalary, VietnameseLaborRegulations.MaxSalaryCapBhtn);
        }
        return baseSalary;
    }

    /// <summary>
    /// Kiểm tra tỷ lệ có hiệu lực tại ngày xác định hay không.
    /// </summary>
    public bool IsEffectiveAt(DateTime date)
    {
        if (!IsActive) return false;
        if (date.Date < EffectiveFrom.Date) return false;
        if (EffectiveTo.HasValue && date.Date > EffectiveTo.Value.Date) return false;
        return true;
    }
}
