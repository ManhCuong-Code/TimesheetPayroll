namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho hồ sơ mẫu đặc trưng khuôn mặt (128-d Biometric Descriptor Vector) của nhân sự.
/// Chỉ có tài khoản Quản trị viên (ROLE_ADMIN) mới có quyền khởi tạo và quản lý.
/// </summary>
public class EmployeeFaceProfile
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? FullName { get; set; }
    public string? DepartmentName { get; set; }
    
    /// <summary>
    /// Chuỗi JSON biểu diễn vector 128 chiều trích xuất từ khuôn mặt (Face Embedding Vector).
    /// </summary>
    public string FaceDescriptorJson { get; set; } = string.Empty;
    
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public string RegisteredBy { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public decimal LivenessThreshold { get; set; } = 0.70m;
    public string? Notes { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Vô hiệu hóa hoặc tạm khóa đăng ký khuôn mặt.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Kích hoạt lại đăng ký khuôn mặt.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
