using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.CoreBusiness.Services.Interfaces;

/// <summary>
/// Hợp đồng giao diện cho động cơ tính toán tiền lương doanh nghiệp (Payroll Engine).
/// </summary>
public interface IPayrollCalculationService
{
    /// <summary>
    /// Thực hiện tính toán chi tiết phiếu lương cho một nhân viên dựa trên ngày công thực tế,
    /// phụ cấp và các tỷ lệ trích nộp bảo hiểm theo quy định pháp luật.
    /// </summary>
    /// <param name="employee">Hồ sơ nhân viên và thông tin thu nhập theo hợp đồng</param>
    /// <param name="periodId">Mã định danh kỳ tính lương</param>
    /// <param name="actualWorkDays">Số ngày công thực tế đi làm trong tháng</param>
    /// <param name="standardWorkDays">Số ngày công chuẩn của tháng (mặc định 22 ngày)</param>
    /// <param name="rates">Danh sách các mức trích nộp bảo hiểm (BHXH, BHYT, BHTN)</param>
    /// <returns>Đối tượng Payslip chứa các khoản mục chi tiết (PayslipLine)</returns>
    Payslip CalculatePayslip(
        Employee employee, 
        long periodId, 
        decimal actualWorkDays, 
        decimal standardWorkDays, 
        List<DeductionRate> rates);

    /// <summary>
    /// Tính toán chi tiết phiếu lương mở rộng, bao gồm ngày nghỉ phép hưởng lương, làm thêm giờ (OT),
    /// thưởng KPI, phạt kỷ luật và tạm tính thuế thu nhập cá nhân (TNCN).
    /// </summary>
    Payslip CalculatePayslip(
        Employee employee,
        long periodId,
        decimal actualWorkDays,
        decimal standardWorkDays,
        List<DeductionRate> rates,
        decimal paidLeaveDays,
        decimal overtimeHours,
        decimal bonusAmount,
        decimal penaltyAmount,
        bool calculateTax = false);

    /// <summary>
    /// Tính toán chi tiết phiếu lương với cơ chế phân bổ lương riêng biệt giữa Ca 8 tiếng và Ca gãy (Split Shift),
    /// tự động phân tách số ngày ca 8h, số ngày ca gãy và áp dụng phụ cấp đặc thù ca gãy (Split Shift Allowance).
    /// </summary>
    Payslip CalculatePayslip(
        Employee employee,
        long periodId,
        decimal standard8hDays,
        decimal splitShiftDays,
        decimal otherShiftDays,
        decimal standardWorkDays,
        List<DeductionRate> rates,
        decimal paidLeaveDays,
        decimal overtimeHours,
        decimal bonusAmount,
        decimal penaltyAmount,
        decimal splitShiftAllowancePerDay = 50000m,
        bool calculateTax = true);
}
