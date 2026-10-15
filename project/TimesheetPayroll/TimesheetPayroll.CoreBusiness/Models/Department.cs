using TimesheetPayroll.CoreBusiness.Exceptions;

namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho cơ cấu phòng ban trong doanh nghiệp.
/// </summary>
public class Department
{
    public long Id { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cập nhật thông tin quản lý phòng ban.
    /// </summary>
    public void AssignManager(string managerName)
    {
        if (string.IsNullOrWhiteSpace(managerName))
            throw new BusinessRuleValidationException("Tên người quản lý phòng ban không được để trống.", nameof(AssignManager));

        ManagerName = managerName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Thay đổi tên và mã phòng ban.
    /// </summary>
    public void UpdateInfo(string code, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessRuleValidationException("Mã phòng ban không được để trống.", nameof(UpdateInfo));
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException("Tên phòng ban không được để trống.", nameof(UpdateInfo));

        DepartmentCode = code.Trim().ToUpperInvariant();
        DepartmentName = name.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
