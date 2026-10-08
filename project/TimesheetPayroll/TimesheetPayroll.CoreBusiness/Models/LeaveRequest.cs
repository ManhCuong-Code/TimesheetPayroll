namespace TimesheetPayroll.CoreBusiness.Models;

public class LeaveRequest
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public LeaveType LeaveType { get; set; } = LeaveType.ANNUAL_LEAVE;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today;
    public decimal TotalDays { get; set; } = 1.0m;
    public string Reason { get; set; } = string.Empty;
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.PENDING;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectReason { get; set; }
}

public class DeductionRate
{
    public long Id { get; set; }
    public string RateCode { get; set; } = string.Empty;
    public string RateName { get; set; } = string.Empty;
    public decimal EmployeeRate { get; set; } // Ví dụ: 0.08m (8%)
    public decimal EmployerRate { get; set; } // Ví dụ: 0.175m (17.5%)
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}

