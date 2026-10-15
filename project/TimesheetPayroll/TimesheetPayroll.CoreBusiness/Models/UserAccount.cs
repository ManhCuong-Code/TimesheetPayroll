using TimesheetPayroll.CoreBusiness.Exceptions;

namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho tài khoản người dùng đăng nhập và phân quyền trong hệ thống (RBAC).
/// </summary>
public class UserAccount
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "ROLE_EMPLOYEE";
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Thông tin mở rộng liên kết từ hồ sơ nhân sự
    public string? FullName { get; set; }
    public string? EmployeeCode { get; set; }
    public string? DepartmentName { get; set; }
    public string? Email { get; set; }

    /// <summary>
    /// Kiểm tra người dùng có quyền Quản trị viên tối cao hay không.
    /// </summary>
    public bool IsAdmin() => Role == "ROLE_ADMIN";

    /// <summary>
    /// Kiểm tra người dùng có quyền Kế toán / Nhân sự hay không.
    /// </summary>
    public bool IsHrAccountant() => Role == "ROLE_HR_ACCOUNTANT" || Role == "ROLE_ADMIN";

    /// <summary>
    /// Kiểm tra người dùng có vai trò Nhân viên hay không.
    /// </summary>
    public bool IsEmployee() => Role == "ROLE_EMPLOYEE";

    /// <summary>
    /// Cập nhật mật khẩu băm mới cho tài khoản.
    /// </summary>
    public void ChangePasswordHash(string newHash)
    {
        if (string.IsNullOrWhiteSpace(newHash))
            throw new BusinessRuleValidationException("Chuỗi băm mật khẩu mới không được để trống.", nameof(ChangePasswordHash));

        PasswordHash = newHash;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ghi nhận thời điểm đăng nhập thành công gần nhất.
    /// </summary>
    public void RecordLoginSuccess()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Vô hiệu hóa tài khoản (khóa đăng nhập).
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mở khóa kích hoạt lại tài khoản.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Phân lại vai trò người dùng (Role-Based Access Control).
    /// </summary>
    public void AssignRole(UserRole newRole)
    {
        Role = newRole.ToString();
        UpdatedAt = DateTime.UtcNow;
    }
}
