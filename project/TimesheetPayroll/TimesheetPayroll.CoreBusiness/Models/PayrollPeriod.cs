namespace TimesheetPayroll.CoreBusiness.Models;

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
}
