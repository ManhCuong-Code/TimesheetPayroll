using System.ComponentModel.DataAnnotations;
using TimesheetPayroll.CoreBusiness.Exceptions;

namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho đơn xin nghỉ phép của nhân viên gửi tới phòng nhân sự xét duyệt.
/// Có tích hợp các DataAnnotations để kiểm thực biểu mẫu EditForm Blazor.
/// </summary>
public class LeaveRequest
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? DepartmentName { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn loại hình nghỉ phép.")]
    public LeaveType LeaveType { get; set; } = LeaveType.ANNUAL_LEAVE;

    [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu nghỉ.")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc nghỉ.")]
    public DateTime EndDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Vui lòng nhập số ngày nghỉ dự kiến.")]
    [Range(0.5, 30.0, ErrorMessage = "Số ngày nghỉ phải nằm trong khoảng từ 0.5 đến 30 ngày.")]
    public decimal TotalDays { get; set; } = 1.0m;

    [Required(ErrorMessage = "Vui lòng ghi rõ lý do xin nghỉ phép.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Lý do xin nghỉ phép phải từ 5 đến 500 ký tự.")]
    public string Reason { get; set; } = string.Empty;
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.PENDING;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Kiểm tra loại nghỉ phép có được hưởng nguyên lương hay không (Nghỉ phép năm hưởng nguyên lương).
    /// </summary>
    public bool IsPaidLeave() => LeaveType == LeaveType.ANNUAL_LEAVE;

    /// <summary>
    /// Kiểm tra đơn có đang chờ duyệt hay không.
    /// </summary>
    public bool CanBeReviewed() => ApprovalStatus == ApprovalStatus.PENDING;

    /// <summary>
    /// Thực hiện phê duyệt đơn nghỉ phép.
    /// </summary>
    public void Approve(string reviewer)
    {
        if (string.IsNullOrWhiteSpace(reviewer))
            throw new BusinessRuleValidationException("Người duyệt đơn không được để trống.", nameof(Approve));

        if (ApprovalStatus != ApprovalStatus.PENDING)
            throw new LeaveRequestException($"Đơn nghỉ phép ID={Id} đã ở trạng thái {ApprovalStatus}, không thể phê duyệt lại.");

        ApprovalStatus = ApprovalStatus.APPROVED;
        ApprovedBy = reviewer.Trim();
        ApprovedAt = DateTime.UtcNow;
        RejectReason = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Thực hiện từ chối đơn nghỉ phép kèm lý do.
    /// </summary>
    public void Reject(string reviewer, string reason)
    {
        if (string.IsNullOrWhiteSpace(reviewer))
            throw new BusinessRuleValidationException("Người duyệt đơn không được để trống.", nameof(Reject));
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleValidationException("Lý do từ chối đơn nghỉ phép không được để trống.", nameof(Reject));

        if (ApprovalStatus != ApprovalStatus.PENDING)
            throw new LeaveRequestException($"Đơn nghỉ phép ID={Id} đã ở trạng thái {ApprovalStatus}, không thể từ chối.");

        ApprovalStatus = ApprovalStatus.REJECTED;
        ApprovedBy = reviewer.Trim();
        ApprovedAt = DateTime.UtcNow;
        RejectReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Tính toán số ngày nghỉ dựa trên StartDate và EndDate (loại trừ các ngày cuối tuần nếu có).
    /// </summary>
    public decimal CalculateWorkingDays()
    {
        if (EndDate.Date < StartDate.Date)
            throw new BusinessRuleValidationException("Ngày kết thúc nghỉ phép không thể trước ngày bắt đầu.", nameof(CalculateWorkingDays));

        int days = 0;
        for (var date = StartDate.Date; date <= EndDate.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
            {
                days++;
            }
        }

        TotalDays = Math.Max(1, days);
        return TotalDays;
    }
}
