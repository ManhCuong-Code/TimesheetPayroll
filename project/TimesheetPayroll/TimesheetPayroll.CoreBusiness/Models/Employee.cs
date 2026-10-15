using TimesheetPayroll.CoreBusiness.Exceptions;

namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho hồ sơ nhân sự cốt lõi của doanh nghiệp.
/// Chứa thông tin định danh, tài khoản ngân hàng chi trả và mức thu nhập theo hợp đồng.
/// </summary>
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

    // Mức thu nhập và phụ cấp liên kết từ hợp đồng lao động
    public decimal BaseSalary { get; set; } = 10000000m;
    public decimal MealAllowance { get; set; } = 500000m;
    public decimal FuelAllowance { get; set; } = 0m;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Kiểm tra nhân viên có đang trong trạng thái làm việc tích cực hay không.
    /// </summary>
    public bool IsActiveEmployee() => string.Equals(Status, "ACTIVE", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Tổng các khoản phụ cấp hàng tháng (ăn trưa + xăng xe).
    /// </summary>
    public decimal TotalAllowances => MealAllowance + FuelAllowance;

    /// <summary>
    /// Tính thâm niên làm việc của nhân viên tính theo năm.
    /// </summary>
    public decimal CalculateTenureYears(DateTime? asOfDate = null)
    {
        var targetDate = asOfDate ?? DateTime.Today;
        if (targetDate < HireDate) return 0m;
        var totalDays = (decimal)(targetDate - HireDate).TotalDays;
        return Math.Round(totalDays / 365.25m, 1);
    }

    /// <summary>
    /// Cập nhật thông tin tài khoản ngân hàng chi trả lương.
    /// </summary>
    public void UpdateBankAccount(string accountNumber, string bankName)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new BusinessRuleValidationException("Số tài khoản ngân hàng không được để trống.", nameof(UpdateBankAccount));
        if (string.IsNullOrWhiteSpace(bankName))
            throw new BusinessRuleValidationException("Tên ngân hàng thụ hưởng không được để trống.", nameof(UpdateBankAccount));

        BankAccountNumber = accountNumber.Trim();
        BankName = bankName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cập nhật mức lương và phụ cấp theo phụ lục hợp đồng mới.
    /// </summary>
    public void UpdateSalaryAndAllowances(decimal baseSalary, decimal mealAllowance, decimal fuelAllowance)
    {
        if (baseSalary < 0)
            throw new BusinessRuleValidationException("Mức lương cơ bản không thể âm.", nameof(UpdateSalaryAndAllowances));
        if (mealAllowance < 0 || fuelAllowance < 0)
            throw new BusinessRuleValidationException("Các khoản phụ cấp không thể âm.", nameof(UpdateSalaryAndAllowances));

        BaseSalary = baseSalary;
        MealAllowance = mealAllowance;
        FuelAllowance = fuelAllowance;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Chuyển đổi phòng ban công tác.
    /// </summary>
    public void AssignDepartment(long departmentId, string? departmentName = null)
    {
        DepartmentId = departmentId;
        DepartmentName = departmentName;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Đánh dấu nhân sự đã thôi việc / chấm dứt hợp đồng lao động.
    /// </summary>
    public void MarkAsResigned()
    {
        Status = "RESIGNED";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Đánh dấu nhân sự đang tạm hoãn hợp đồng / nghỉ thai sản / nghỉ dài hạn.
    /// </summary>
    public void MarkAsOnLeave()
    {
        Status = "ON_LEAVE";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Kích hoạt lại trạng thái hoạt động bình thường.
    /// </summary>
    public void Reactivate()
    {
        Status = "ACTIVE";
        UpdatedAt = DateTime.UtcNow;
    }
}
