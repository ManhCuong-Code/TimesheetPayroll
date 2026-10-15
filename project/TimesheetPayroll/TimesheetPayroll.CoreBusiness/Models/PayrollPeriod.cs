using TimesheetPayroll.CoreBusiness.Exceptions;

namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Đại diện cho một kỳ tính lương hàng tháng trong doanh nghiệp.
/// Triển khai đầy đủ máy trạng thái (State Machine theo UML 12_state_payroll_period):
/// DRAFT -> CALCULATING -> CALCULATED -> LOCKED -> PAID.
/// </summary>
public class PayrollPeriod
{
    public long Id { get; set; }
    public string PeriodCode { get; set; } = string.Empty;
    public int PeriodMonth { get; set; }
    public int PeriodYear { get; set; }
    public decimal StandardWorkDays { get; set; } = 22.0m;
    public PeriodStatus Status { get; set; } = PeriodStatus.DRAFT;
    public decimal TotalGrossAmount { get; set; } = 0m;
    public decimal TotalNetAmount { get; set; } = 0m;
    public DateTime? LockedAt { get; set; }
    public string? LockedBy { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Kiểm tra kỳ lương có đủ điều kiện để tính toán hay không (DRAFT hoặc CALCULATED).
    /// </summary>
    public bool CanCalculate() => Status == PeriodStatus.DRAFT || Status == PeriodStatus.CALCULATED;

    /// <summary>
    /// Kiểm tra kỳ lương đã bị khóa sổ (Read-Only) hay chưa (LOCKED hoặc PAID).
    /// </summary>
    public bool IsLocked() => Status == PeriodStatus.LOCKED || Status == PeriodStatus.PAID;

    /// <summary>
    /// Bắt đầu tiến trình tính lương tự động trong Transaction (DRAFT/CALCULATED -> CALCULATING).
    /// </summary>
    public void StartCalculating()
    {
        if (IsLocked())
            throw new InvalidPeriodStateException(PeriodCode, Status, PeriodStatus.CALCULATING);

        Status = PeriodStatus.CALCULATING;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Hoàn tất tính toán thành công 100% cho toàn bộ nhân sự (CALCULATING -> CALCULATED).
    /// </summary>
    public void CompleteCalculation(decimal totalGross, decimal totalNet)
    {
        if (Status != PeriodStatus.CALCULATING && Status != PeriodStatus.CALCULATED && Status != PeriodStatus.DRAFT)
            throw new InvalidPeriodStateException(PeriodCode, Status, PeriodStatus.CALCULATED);

        if (totalGross < 0 || totalNet < 0)
            throw new BusinessRuleValidationException("Tổng quỹ lương không thể là số âm.", nameof(CompleteCalculation));

        Status = PeriodStatus.CALCULATED;
        TotalGrossAmount = totalGross;
        TotalNetAmount = totalNet;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Hoàn tác về trạng thái nháp khi xảy ra lỗi/ngoại lệ trong quá trình tính toán (CALCULATING -> DRAFT).
    /// </summary>
    public void RollbackToDraft()
    {
        if (IsLocked())
            throw new InvalidPeriodStateException(PeriodCode, Status, PeriodStatus.DRAFT);

        Status = PeriodStatus.DRAFT;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Khóa sổ kỳ lương vĩnh viễn (CALCULATED -> LOCKED), bảo vệ tính toàn vẹn dữ liệu Read-Only.
    /// </summary>
    public void Lock(string lockedByUsername)
    {
        if (string.IsNullOrWhiteSpace(lockedByUsername))
            throw new BusinessRuleValidationException("Người thực hiện khóa sổ không được để trống.", nameof(Lock));

        if (Status == PeriodStatus.LOCKED || Status == PeriodStatus.PAID)
            return; // Đã khóa trước đó

        if (Status != PeriodStatus.CALCULATED)
            throw new InvalidPeriodStateException(PeriodCode, Status, PeriodStatus.LOCKED);

        Status = PeriodStatus.LOCKED;
        LockedAt = DateTime.UtcNow;
        LockedBy = lockedByUsername.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mở khóa kỳ lương (LOCKED -> CALCULATED) để cho phép điều chỉnh chấm công hoặc tính toán lại bảng lương.
    /// </summary>
    public void Unlock()
    {
        if (Status == PeriodStatus.PAID)
            throw new InvalidPeriodStateException(PeriodCode, Status, PeriodStatus.CALCULATED);

        Status = PeriodStatus.CALCULATED;
        LockedAt = null;
        LockedBy = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Đánh dấu kỳ lương đã hoàn tất chi trả ngân hàng (LOCKED -> PAID).
    /// </summary>
    public void MarkAsPaid()
    {
        if (Status == PeriodStatus.PAID)
            return;

        if (Status != PeriodStatus.LOCKED)
            throw new InvalidPeriodStateException(PeriodCode, Status, PeriodStatus.PAID);

        Status = PeriodStatus.PAID;
        PaidAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cho phép mở lại kỳ lương để tính toán lại khi đang ở trạng thái CALCULATED.
    /// </summary>
    public void ReopenForRecalculation()
    {
        if (IsLocked())
            throw new InvalidPeriodStateException(PeriodCode, Status, PeriodStatus.DRAFT);

        Status = PeriodStatus.DRAFT;
        UpdatedAt = DateTime.UtcNow;
    }
}
