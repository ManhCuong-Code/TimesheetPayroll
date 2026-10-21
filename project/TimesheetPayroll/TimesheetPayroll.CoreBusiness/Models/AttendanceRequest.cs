namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho yêu cầu chấm công thủ công (bổ sung giờ vào/ra, điều chỉnh thời gian) của nhân viên.
/// </summary>
public class AttendanceRequest
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime WorkDate { get; set; } = DateTime.Today;
    public string RequestType { get; set; } = "BOTH"; // CHECK_IN_ONLY, CHECK_OUT_ONLY, BOTH
    public TimeSpan? ProposedCheckIn { get; set; }
    public TimeSpan? ProposedCheckOut { get; set; }
    public long? ShiftId { get; set; }
    public string? ShiftName { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? EvidenceFileName { get; set; }
    public string ApprovalStatus { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsPending => string.Equals(ApprovalStatus, "PENDING", StringComparison.OrdinalIgnoreCase);
    public bool IsApproved => string.Equals(ApprovalStatus, "APPROVED", StringComparison.OrdinalIgnoreCase);
    public bool IsRejected => string.Equals(ApprovalStatus, "REJECTED", StringComparison.OrdinalIgnoreCase);

    public string GetRequestTypeDisplay() => RequestType switch
    {
        "CHECK_IN_ONLY" => "Bổ sung giờ vào",
        "CHECK_OUT_ONLY" => "Bổ sung giờ ra",
        "BOTH" => "Điều chỉnh cả giờ vào & ra",
        _ => "Điều chỉnh giờ công"
    };

    public string GetStatusBadgeClass() => ApprovalStatus switch
    {
        "APPROVED" => "bg-success text-white",
        "REJECTED" => "bg-danger text-white",
        _ => "bg-warning text-dark"
    };

    public string GetStatusDisplay() => ApprovalStatus switch
    {
        "APPROVED" => "Đã duyệt",
        "REJECTED" => "Đã từ chối",
        _ => "Chờ duyệt"
    };
}
