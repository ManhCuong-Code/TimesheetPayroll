namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Dòng chi tiết thành phần thu nhập hoặc khấu trừ cấu thành nên phiếu lương.
/// </summary>
public class PayslipLine
{
    public long Id { get; set; }
    public long PayslipId { get; set; }
    public PayslipLineType LineType { get; set; }
    public string LineDescription { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsDeduction { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Hiển thị số tiền có định dạng dấu cộng (+) cho thu nhập và dấu trừ (-) cho khoản khấu trừ.
    /// </summary>
    public string FormattedSignedAmount => IsDeduction 
        ? $"-{Amount:N0} đ" 
        : $"+{Amount:N0} đ";
}
