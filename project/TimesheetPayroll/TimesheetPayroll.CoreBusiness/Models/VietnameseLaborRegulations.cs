namespace TimesheetPayroll.CoreBusiness.Models;

/// <summary>
/// Các quy định pháp luật lao động, tiền lương, bảo hiểm và thuế mới nhất của Việt Nam.
/// Áp dụng theo:
/// - Nghị định 73/2024/NĐ-CP (Lương cơ sở và mức trần BHXH, BHYT)
/// - Nghị định 74/2024/NĐ-CP (Lương tối thiểu vùng và mức trần BHTN)
/// - Nghị quyết 954/2020/UBTVQH14 & Thông tư 111/2013/TT-BTC (Thuế TNCN và Giảm trừ gia cảnh)
/// - Thông tư 26/2016/TT-BLĐTBXH (Mức phụ cấp ăn trưa miễn thuế)
/// - Điều 85, 86 Luật Bảo hiểm Xã hội 2014 & Quyết định 595/QĐ-BHXH (Quy tắc 14 ngày nghỉ không lương)
/// - Điều 98 Bộ luật Lao động 2019 & Nghị định 145/2020/NĐ-CP (Làm thêm giờ OT)
/// </summary>
public static class VietnameseLaborRegulations
{
    /// <summary>
    /// Mức lương cơ sở mới nhất theo Nghị định 73/2024/NĐ-CP (hiệu lực từ 01/07/2024): 2,340,000 VNĐ/tháng.
    /// </summary>
    public const decimal BaseStatutorySalary = 2340000m;

    /// <summary>
    /// Mức trần tiền lương tháng đóng BHXH và BHYT (tối đa bằng 20 lần mức lương cơ sở)
    /// theo Nghị định 73/2024/NĐ-CP: 20 x 2,340,000 = 46,800,000 VNĐ/tháng.
    /// </summary>
    public const decimal MaxSalaryCapBhxhBhyt = 46800000m;

    /// <summary>
    /// Mức lương tối thiểu Vùng I theo Nghị định 74/2024/NĐ-CP (hiệu lực từ 01/07/2024): 4,960,000 VNĐ/tháng.
    /// </summary>
    public const decimal RegionalMinimumSalaryRegion1 = 4960000m;

    /// <summary>
    /// Mức trần tiền lương tháng đóng Bảo hiểm thất nghiệp (BHTN) (tối đa bằng 20 lần mức lương tối thiểu vùng I)
    /// theo Nghị định 74/2024/NĐ-CP: 20 x 4,960,000 = 99,200,000 VNĐ/tháng.
    /// </summary>
    public const decimal MaxSalaryCapBhtn = 99200000m;

    /// <summary>
    /// Mức giảm trừ gia cảnh cho bản thân người nộp thuế theo quy định mới nhất: 15,000,000 VNĐ/tháng (thu nhập từ 15 triệu mới bắt đầu tính thuế).
    /// </summary>
    public const decimal PersonalTaxDeduction = 15000000m;

    /// <summary>
    /// Mức giảm trừ gia cảnh cho mỗi người phụ thuộc theo Nghị quyết 954/2020/UBTVQH14: 4,400,000 VNĐ/người/tháng.
    /// </summary>
    public const decimal DependentTaxDeduction = 4400000m;

    /// <summary>
    /// Mức tiền phụ cấp ăn trưa/giữa ca tối đa được miễn thuế TNCN theo Thông tư 26/2016/TT-BLĐTBXH: 730,000 VNĐ/tháng.
    /// (Phần chi vượt quá 730,000 đ/tháng tính vào thu nhập chịu thuế).
    /// </summary>
    public const decimal MaxNonTaxableMealAllowance = 730000m;

    /// <summary>
    /// Tỷ lệ trích nộp bảo hiểm phần người lao động đóng: 8% BHXH + 1.5% BHYT + 1% BHTN = 10.5%.
    /// </summary>
    public const decimal EmployeeTotalInsuranceRate = 0.105m;

    /// <summary>
    /// Tỷ lệ trích nộp bảo hiểm phần người sử dụng lao động (doanh nghiệp) đóng: 17.5% BHXH + 3% BHYT + 1% BHTN = 21.5%.
    /// </summary>
    public const decimal EmployerTotalInsuranceRate = 0.215m;

    /// <summary>
    /// Số ngày nghỉ phép hàng năm cơ bản cho người lao động làm việc trong điều kiện bình thường (Điều 113 Bộ luật Lao động 2019): 12 ngày/năm.
    /// Cứ mỗi đủ 05 năm làm việc cho một người sử dụng lao động thì số ngày nghỉ hằng năm được tăng thêm tương ứng 01 ngày (Điều 114).
    /// </summary>
    public const int BasicAnnualLeaveDaysPerYear = 12;

    /// <summary>
    /// Số năm thâm niên để được cộng thêm 1 ngày phép năm: 5 năm.
    /// </summary>
    public const int SeniorityYearsPerBonusLeaveDay = 5;

    /// <summary>
    /// Tổng số ngày nghỉ Lễ, Tết trong năm được hưởng nguyên lương theo Điều 112 Bộ luật Lao động 2019: 11 ngày.
    /// (Tết Dương lịch 1, Tết Âm lịch 5, Giỗ Tổ Hùng Vương 1, 30/4 1, 1/5 1, Quốc khánh 2/9 2 ngày).
    /// </summary>
    public const int TotalPaidPublicHolidays = 11;

    /// <summary>
    /// Số ngày nghỉ việc riêng có hưởng nguyên lương theo Điều 115 Bộ luật Lao động 2019:
    /// - Kết hôn: nghỉ 03 ngày.
    /// - Con đẻ, con nuôi kết hôn: nghỉ 01 ngày.
    /// - Cha/mẹ đẻ, cha/mẹ nuôi, cha/mẹ vợ/chồng, vợ/chồng, con đẻ, con nuôi chết: nghỉ 03 ngày.
    /// </summary>
    public const int MarriageLeaveDays = 3;
    public const int ChildMarriageLeaveDays = 1;
    public const int BereavementLeaveDays = 3;

    /// <summary>
    /// Tỷ lệ trả thêm tối thiểu khi làm việc vào ban đêm (từ 22h đêm đến 6h sáng hôm sau) theo Điều 98 BLLĐ: ít nhất 30% tiền lương giờ.
    /// </summary>
    public const decimal NightShiftBonusRate = 0.30m;

    /// <summary>
    /// Tỷ lệ trả thêm khi làm thêm giờ vào ban đêm: ít nhất thêm 20% tiền lương tính theo công việc làm thêm vào ban ngày.
    /// </summary>
    public const decimal NightOvertimeBonusRate = 0.20m;

    /// <summary>
    /// Tiền lương trong thời gian thử việc tối thiểu theo Điều 26 BLLĐ: ít nhất bằng 85% mức lương của công việc đó.
    /// </summary>
    public const decimal ProbationSalaryMinimumRate = 0.85m;

    /// <summary>
    /// Ngưỡng số ngày nghỉ việc không hưởng lương trong tháng để không phải đóng bảo hiểm xã hội (Điều 85 Luật BHXH 2014 & QĐ 595/QĐ-BHXH): từ 14 ngày làm việc trở lên.
    /// </summary>
    public const int UnpaidLeaveDaysThresholdForInsuranceExemption = 14;
}
