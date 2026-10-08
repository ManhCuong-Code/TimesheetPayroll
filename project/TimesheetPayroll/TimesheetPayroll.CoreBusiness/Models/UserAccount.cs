namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho tài khoản người dùng đăng nhập và phân quyền trong hệ thống.
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

    // Thông tin mở rộng liên kết từ hồ sơ nhân viên
    public string? FullName { get; set; }
    public string? EmployeeCode { get; set; }
    public string? DepartmentName { get; set; }
    public string? Email { get; set; }
}
