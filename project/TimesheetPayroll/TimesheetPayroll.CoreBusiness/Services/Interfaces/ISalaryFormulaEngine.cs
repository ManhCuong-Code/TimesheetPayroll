using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.CoreBusiness.Services.Interfaces;

/// <summary>
/// Động cơ công thức toán học tính lương (Salary Formula Engine).
/// Triển khai các thuật toán nguyên tử theo quy chuẩn Luật Lao động và Thuế TNCN Việt Nam.
/// </summary>
public interface ISalaryFormulaEngine
{
    /// <summary>
    /// Tính lương ngày công thực tế theo tỷ lệ:
    /// Lương ngày công = Lương cơ bản * (Ngày công thực tế / Ngày công chuẩn).
    /// </summary>
    decimal ComputeProratedBaseSalary(decimal baseSalary, decimal actualWorkDays, decimal standardWorkDays);

    /// <summary>
    /// Tính tiền lương ngày nghỉ phép có hưởng lương (phép năm):
    /// Tiền phép năm = (Lương cơ bản / Ngày công chuẩn) * Số ngày nghỉ phép năm.
    /// </summary>
    decimal ComputePaidLeavePay(decimal baseSalary, decimal paidLeaveDays, decimal standardWorkDays);

    /// <summary>
    /// Tính tiền làm thêm giờ (Overtime):
    /// Tiền OT = (Lương cơ bản / (Ngày công chuẩn * 8 giờ)) * Số giờ làm thêm * Hệ số làm thêm (mặc định 1.5).
    /// </summary>
    decimal ComputeOvertimePay(decimal baseSalary, decimal standardWorkDays, decimal overtimeHours, decimal overtimeMultiplier = 1.5m);

    /// <summary>
    /// Tính tổng thu nhập gộp (Gross Salary):
    /// Gross = Lương ngày công + Lương phép năm + Tiền OT + Phụ cấp + Tiền thưởng.
    /// </summary>
    decimal ComputeGrossSalary(
        decimal proratedBaseSalary, 
        decimal paidLeavePay, 
        decimal overtimePay, 
        decimal allowances, 
        decimal bonus = 0m);

    /// <summary>
    /// Tính chi tiết các khoản trích nộp bảo hiểm bắt buộc của người lao động (BHXH, BHYT, BHTN).
    /// </summary>
    Dictionary<string, decimal> ComputeMandatoryDeductions(decimal baseSalary, IEnumerable<DeductionRate>? rates);

    /// <summary>
    /// Tính thuế thu nhập cá nhân (TNCN) tạm tính theo biểu thuế lũy tiến từng phần của Việt Nam:
    /// Giảm trừ gia cảnh bản thân mức mới: 15,000,000 đ/tháng.
    /// Thu nhập tính thuế = Thu nhập chịu thuế (Gross - Phụ cấp ăn trưa miễn thuế - Bảo hiểm bắt buộc) - 15,000,000 đ.
    /// </summary>
    decimal ComputePersonalIncomeTax(decimal grossSalary, decimal nonTaxableAllowances, decimal totalInsuranceDeductions, decimal personalDeduction = 15000000m);

    /// <summary>
    /// Tính tiền lương thực lĩnh chuyển khoản (Net Salary):
    /// Net = Gross - Tổng các khoản khấu trừ (Bảo hiểm, Thuế, Phạt vi phạm).
    /// </summary>
    decimal ComputeNetSalary(decimal grossSalary, decimal totalDeductions);
}
