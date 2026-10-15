using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;

namespace TimesheetPayroll.CoreBusiness.Services;

/// <summary>
/// Động cơ tính toán lương tự động cho doanh nghiệp (Payroll Calculation Service).
/// Triển khai phối hợp các công thức nguyên tử thông qua SalaryFormulaEngine
/// theo đúng quy định của Bộ luật Lao động và Luật Bảo hiểm Xã hội Việt Nam.
/// </summary>
public class PayrollCalculationService : IPayrollCalculationService
{
    private readonly ISalaryFormulaEngine _formulaEngine;

    public PayrollCalculationService(ISalaryFormulaEngine? formulaEngine = null)
    {
        _formulaEngine = formulaEngine ?? new SalaryFormulaEngine();
    }

    /// <summary>
    /// Phương thức tương thích ngược: Tính toán phiếu lương cơ bản dựa trên ngày công thực tế và tỷ lệ trích nộp bảo hiểm.
    /// Tự động tính toán thuế TNCN theo luật định.
    /// </summary>
    public Payslip CalculatePayslip(
        Employee employee,
        long periodId,
        decimal actualWorkDays,
        decimal standardWorkDays,
        List<DeductionRate> rates)
    {
        return CalculatePayslip(
            employee,
            periodId,
            actualWorkDays,
            standardWorkDays,
            rates,
            paidLeaveDays: 0m,
            overtimeHours: 0m,
            bonusAmount: 0m,
            penaltyAmount: 0m,
            calculateTax: true);
    }

    /// <summary>
    /// Phương thức tính lương cơ bản tương thích ngược.
    /// </summary>
    public Payslip CalculatePayslip(
        Employee employee,
        long periodId,
        decimal actualWorkDays,
        decimal standardWorkDays,
        List<DeductionRate> rates,
        decimal paidLeaveDays,
        decimal overtimeHours,
        decimal bonusAmount,
        decimal penaltyAmount,
        bool calculateTax = true)
    {
        return CalculatePayslip(
            employee,
            periodId,
            standard8hDays: actualWorkDays,
            splitShiftDays: 0m,
            otherShiftDays: 0m,
            standardWorkDays: standardWorkDays,
            rates: rates,
            paidLeaveDays: paidLeaveDays,
            overtimeHours: overtimeHours,
            bonusAmount: bonusAmount,
            penaltyAmount: penaltyAmount,
            splitShiftAllowancePerDay: 50000m,
            calculateTax: calculateTax);
    }

    /// <summary>
    /// Phương thức tính lương mở rộng đầy đủ với cơ chế PHÂN BỔ PHÂN LƯƠNG RIÊNG BIỆT giữa Ca 8 tiếng và Ca gãy.
    /// Bao gồm tiền công theo từng loại ca, phụ cấp ca gãy (Split Shift Allowance), OT, phép năm, bảo hiểm và thuế TNCN.
    /// </summary>
    public Payslip CalculatePayslip(
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
        bool calculateTax = true)
    {
        ArgumentNullException.ThrowIfNull(employee);

        if (standardWorkDays <= 0) standardWorkDays = 22.0m;
        if (standard8hDays < 0) standard8hDays = 0m;
        if (splitShiftDays < 0) splitShiftDays = 0m;
        if (otherShiftDays < 0) otherShiftDays = 0m;
        if (paidLeaveDays < 0) paidLeaveDays = 0m;
        if (overtimeHours < 0) overtimeHours = 0m;

        decimal actualWorkDays = standard8hDays + splitShiftDays + otherShiftDays;

        var payslip = new Payslip
        {
            PeriodId = periodId,
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = employee.FullName,
            DepartmentName = employee.DepartmentName,
            BankAccountNumber = employee.BankAccountNumber,
            BankName = employee.BankName,
            ActualWorkDays = actualWorkDays,
            PaidLeaveDays = paidLeaveDays,
            OvertimeHours = overtimeHours
        };

        // 1. Phân bố phân lương theo từng loại ca làm việc:
        if (splitShiftDays > 0)
        {
            // Có phát sinh ca gãy: tách bạch rõ ràng cấu phần lương ca 8h và ca gãy
            if (standard8hDays > 0)
            {
                decimal standard8hPay = _formulaEngine.ComputeProratedBaseSalary(
                    employee.BaseSalary, standard8hDays, standardWorkDays);
                payslip.AddEarningsLine(
                    PayslipLineType.BASE_SALARY_PRORATED,
                    $"Lương ngày công ca 8 tiếng chuẩn ({standard8hDays:0.##}/{standardWorkDays:0.##} ngày)",
                    standard8hPay);
            }

            decimal splitShiftBasePay = _formulaEngine.ComputeProratedBaseSalary(
                employee.BaseSalary, splitShiftDays, standardWorkDays);
            payslip.AddEarningsLine(
                PayslipLineType.BASE_SALARY_PRORATED,
                $"Lương ngày công ca gãy ({splitShiftDays:0.##}/{standardWorkDays:0.##} ngày)",
                splitShiftBasePay);

            // Phụ cấp đặc thù ca gãy
            decimal splitAllowanceTotal = splitShiftDays * splitShiftAllowancePerDay;
            if (splitAllowanceTotal > 0)
            {
                payslip.AddEarningsLine(
                    PayslipLineType.SPLIT_SHIFT_ALLOWANCE,
                    $"Phụ cấp làm việc ca gãy ({splitShiftDays:0.##} ca x {splitShiftAllowancePerDay:N0} đ/ca)",
                    splitAllowanceTotal);
            }

            if (otherShiftDays > 0)
            {
                decimal otherPay = _formulaEngine.ComputeProratedBaseSalary(
                    employee.BaseSalary, otherShiftDays, standardWorkDays);
                payslip.AddEarningsLine(
                    PayslipLineType.BASE_SALARY_PRORATED,
                    $"Lương ca nửa ngày / khác ({otherShiftDays:0.##}/{standardWorkDays:0.##} ngày)",
                    otherPay);
            }
        }
        else
        {
            // Trường hợp không làm ca gãy (chỉ làm ca 8h hoặc ca nửa ngày)
            decimal proratedBaseSalary = _formulaEngine.ComputeProratedBaseSalary(
                employee.BaseSalary, actualWorkDays, standardWorkDays);

            payslip.AddEarningsLine(
                PayslipLineType.BASE_SALARY_PRORATED,
                $"Lương ngày công ca 8 tiếng ({actualWorkDays:0.##}/{standardWorkDays:0.##} ngày)",
                proratedBaseSalary);
        }

        // 2. Tiền lương ngày nghỉ phép có hưởng lương (phép năm)
        decimal paidLeavePay = 0m;
        if (paidLeaveDays > 0)
        {
            paidLeavePay = _formulaEngine.ComputePaidLeavePay(
                employee.BaseSalary, paidLeaveDays, standardWorkDays);

            payslip.AddEarningsLine(
                PayslipLineType.PAID_LEAVE_PAY,
                $"Lương nghỉ phép năm ({paidLeaveDays:0.##} ngày hưởng nguyên lương)",
                paidLeavePay);
        }

        // 3. Tiền làm thêm giờ (Overtime - Điều 98 BLLĐ 2019)
        decimal overtimePay = 0m;
        if (overtimeHours > 0)
        {
            overtimePay = _formulaEngine.ComputeOvertimePay(
                employee.BaseSalary, standardWorkDays, overtimeHours, 1.5m);

            payslip.AddEarningsLine(
                PayslipLineType.OVERTIME_PAY,
                $"Tiền làm thêm giờ OT ({overtimeHours:0.##} giờ - hệ số 150%)",
                overtimePay);
        }

        // 4. Phụ cấp tiền ăn trưa
        if (employee.MealAllowance > 0)
        {
            payslip.AddEarningsLine(
                PayslipLineType.MEAL_ALLOWANCE,
                "Phụ cấp tiền ăn trưa",
                employee.MealAllowance);
        }

        // 5. Phụ cấp tiền xăng xe / đi lại
        if (employee.FuelAllowance > 0)
        {
            payslip.AddEarningsLine(
                PayslipLineType.FUEL_ALLOWANCE,
                "Phụ cấp xăng xe / đi lại",
                employee.FuelAllowance);
        }

        // 6. Tiền thưởng hiệu suất công việc / KPI
        if (bonusAmount > 0)
        {
            payslip.AddEarningsLine(
                PayslipLineType.BONUS,
                "Thưởng hiệu suất công việc",
                bonusAmount);
        }

        // 7. Các khoản trích nộp bảo hiểm bắt buộc theo luật (NĐ 73/2024/NĐ-CP & NĐ 74/2024/NĐ-CP)
        // Quy tắc Điều 85 Luật BHXH 2014 & QĐ 595/QĐ-BHXH:
        // Người lao động không làm việc và không hưởng lương từ 14 ngày làm việc trở lên trong tháng thì không đóng bảo hiểm.
        decimal totalPaidDays = actualWorkDays + paidLeaveDays;
        decimal unpaidDays = Math.Max(0m, standardWorkDays - totalPaidDays);
        bool exemptFromInsurance = unpaidDays >= 14m;

        decimal totalInsurance = 0m;
        if (!exemptFromInsurance && rates != null && rates.Count > 0)
        {
            foreach (var rate in rates.Where(r => r.IsActive))
            {
                decimal deductionAmount = rate.CalculateEmployeeDeduction(employee.BaseSalary);
                if (deductionAmount <= 0) continue;

                totalInsurance += deductionAmount;

                var lineType = rate.RateCode.ToUpperInvariant() switch
                {
                    "BHXH" => PayslipLineType.DEDUCTION_BHXH,
                    "BHYT" => PayslipLineType.DEDUCTION_BHYT,
                    "BHTN" => PayslipLineType.DEDUCTION_BHTN,
                    _ => PayslipLineType.PENALTY
                };

                payslip.AddDeductionLine(
                    lineType,
                    $"Trích nộp {rate.RateName} ({rate.EmployeeRate * 100:0.#}%)",
                    deductionAmount);
            }
        }
        else if (exemptFromInsurance && totalPaidDays < standardWorkDays)
        {
            payslip.Note = "Nghỉ không lương từ 14 ngày trở lên trong tháng, không phải trích nộp bảo hiểm xã hội theo Điều 85 Luật BHXH 2014.";
        }

        // 8. Tạm tính Thuế Thu Nhập Cá Nhân (TNCN) theo biểu lũy tiến TT 111/2013/TT-BTC & NQ 954/2020
        if (calculateTax)
        {
            // Phụ cấp ăn trưa theo luật thuế được miễn tối đa 730,000 đ/tháng (Thông tư 26/2016/TT-BLĐTBXH)
            decimal nonTaxableMeal = Math.Min(employee.MealAllowance, VietnameseLaborRegulations.MaxNonTaxableMealAllowance);

            // Phần tiền làm thêm giờ (OT) trả cao hơn so với bình thường được miễn thuế TNCN (Điểm i Khoản 1 Điều 3 TT 111/2013/TT-BTC)
            decimal nonTaxableOt = 0m;
            if (overtimePay > 0)
            {
                // Hệ số 1.5 thì phần chênh 0.5/1.5 = 1/3 tiền OT được miễn thuế
                nonTaxableOt = Math.Round(overtimePay / 3.0m, 0, MidpointRounding.AwayFromZero);
            }

            decimal totalNonTaxable = nonTaxableMeal + nonTaxableOt;

            decimal taxAmount = _formulaEngine.ComputePersonalIncomeTax(
                payslip.GrossSalary, 
                totalNonTaxable, 
                totalInsurance, 
                VietnameseLaborRegulations.PersonalTaxDeduction);

            if (taxAmount > 0)
            {
                payslip.AddDeductionLine(
                    PayslipLineType.PERSONAL_INCOME_TAX,
                    "Thuế thu nhập cá nhân (TNCN) tạm tính",
                    taxAmount);
            }
        }

        // 9. Phạt vi phạm quy chế / đi muộn
        if (penaltyAmount > 0)
        {
            payslip.AddDeductionLine(
                PayslipLineType.PENALTY,
                "Khấu trừ vi phạm / kỷ luật",
                penaltyAmount);
        }

        // Tổng hợp lại Gross, Deductions, Net
        payslip.RecalculateTotals();

        return payslip;
    }
}
