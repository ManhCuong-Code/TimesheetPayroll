namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho nhật ký kiểm toán sinh trắc học khuôn mặt và các lần chấm công bằng camera.
/// </summary>
public class FaceAuditLog
{
    public long Id { get; set; }
    public long? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? FullName { get; set; }
    
    /// <summary>
    /// Loại hành động: REGISTER, RE_ENROLL, REVOKE, LOCK, UNLOCK, PUNCH_CHECK_IN, PUNCH_CHECK_OUT, VERIFY_FAIL
    /// </summary>
    public string ActionType { get; set; } = string.Empty;
    
    /// <summary>
    /// Điểm tương đồng sinh trắc học hoặc khoảng cách Euclidean (ví dụ: 0.945 hoặc 0.35)
    /// </summary>
    public decimal? SimilarityScore { get; set; }
    
    public string PerformedBy { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
