namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho lịch sử chỉnh sửa / điều chỉnh bản ghi chấm công phục vụ kiểm toán và minh bạch dữ liệu.
/// </summary>
public class TimesheetAuditLog
{
    public long Id { get; set; }
    public long? TimesheetId { get; set; }
    public long EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    
    /// <summary>
    /// Hành động: MANUAL_EDIT, FACE_CHECK_IN, FACE_CHECK_OUT, BUTTON_CHECK_IN, BUTTON_CHECK_OUT, RESET_TIMESHEET
    /// </summary>
    public string Action { get; set; } = string.Empty;
    
    public string? BeforeData { get; set; }
    public string? AfterData { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
}
