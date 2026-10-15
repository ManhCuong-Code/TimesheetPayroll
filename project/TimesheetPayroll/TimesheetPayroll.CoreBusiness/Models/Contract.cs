using TimesheetPayroll.CoreBusiness.Exceptions;

namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Thực thể Hợp đồng lao động ký kết giữa doanh nghiệp và nhân viên.
/// Quy định mức lương cơ bản, các khoản phụ cấp và thời hạn hiệu lực pháp lý.
/// </summary>
public class Contract
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string ContractType { get; set; } = "FULL_TIME";
    public decimal BaseSalary { get; set; } = 10000000m;
    public decimal MealAllowance { get; set; } = 500000m;
    public decimal FuelAllowance { get; set; } = 0m;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Kiểm tra hợp đồng có hiệu lực tại một thời điểm hoặc kỳ tính lương xác định hay không.
    /// </summary>
    public bool IsValidForPeriod(DateTime date)
    {
        if (!IsActive) return false;
        if (date.Date < StartDate.Date) return false;
        if (EndDate.HasValue && date.Date > EndDate.Value.Date) return false;
        return true;
    }

    /// <summary>
    /// Tính đơn giá lương 1 ngày công chuẩn dựa trên số ngày công chuẩn của tháng.
    /// </summary>
    public decimal CalculateDailyStandardRate(decimal standardWorkDays = 22.0m)
    {
        if (standardWorkDays <= 0) standardWorkDays = 22.0m;
        return Math.Round(BaseSalary / standardWorkDays, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Tính đơn giá lương 1 giờ làm việc tiêu chuẩn (8 tiếng/ngày).
    /// </summary>
    public decimal CalculateHourlyStandardRate(decimal standardWorkDays = 22.0m, decimal hoursPerDay = 8.0m)
    {
        if (hoursPerDay <= 0) hoursPerDay = 8.0m;
        var dailyRate = CalculateDailyStandardRate(standardWorkDays);
        return Math.Round(dailyRate / hoursPerDay, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Chấm dứt hiệu lực hợp đồng lao động tại một ngày xác định.
    /// </summary>
    public void Terminate(DateTime endDate)
    {
        if (endDate.Date < StartDate.Date)
            throw new BusinessRuleValidationException("Ngày kết thúc hợp đồng không thể trước ngày bắt đầu.", nameof(Terminate));

        EndDate = endDate.Date;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
