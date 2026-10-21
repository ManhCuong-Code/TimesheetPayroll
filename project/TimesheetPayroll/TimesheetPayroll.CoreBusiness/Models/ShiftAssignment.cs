namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho bản ghi phân công ca làm việc cho nhân viên theo ngày.
/// Hệ thống tự động truy vấn bảng này để xác định ca trực tiếp khi nhân viên quét khuôn mặt.
/// </summary>
public class ShiftAssignment
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? DepartmentName { get; set; }

    public long ShiftId { get; set; }
    public string? ShiftCode { get; set; }
    public string? ShiftName { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsNightShift { get; set; }
    public bool IsOvernight { get; set; }
    public decimal WorkUnits { get; set; } = 1.0m;

    public DateTime WorkDate { get; set; }
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, COMPLETED, CANCELLED
    public string? AssignedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string ShiftDisplay => $"{ShiftName ?? "Ca"} ({StartTime:hh\\:mm} - {EndTime:hh\\:mm})";
}
