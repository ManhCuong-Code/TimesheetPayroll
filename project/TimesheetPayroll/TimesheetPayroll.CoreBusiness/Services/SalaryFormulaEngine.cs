using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;

namespace TimesheetPayroll.CoreBusiness.Services;

/// <summary>
/// Động cơ công thức tính lương độc lập (Salary Formula Engine).
/// Triển khai chuẩn hóa toàn bộ các công thức toán học tính lương, làm thêm giờ, bảo hiểm và thuế TNCN theo pháp luật Việt Nam.
/// </summary>
public class SalaryFormulaEngine : ISalaryFormulaEngine
{
    public decimal ComputeProratedBaseSalary(decimal baseSalary, decimal actualWorkDays, decimal standardWorkDays)
    {
        if (baseSalary <= 0 || actualWorkDays <= 0) return 0m;
        if (standardWorkDays <= 0) standardWorkDays = 22.0m;

        decimal amount = baseSalary * (actualWorkDays / standardWorkDays);
        return Math.Round(amount, 0, MidpointRounding.AwayFromZero);
    }

    public decimal ComputePaidLeavePay(decimal baseSalary, decimal paidLeaveDays, decimal standardWorkDays)
    {
        if (baseSalary <= 0 || paidLeaveDays <= 0) return 0m;
        if (standardWorkDays <= 0) standardWorkDays = 22.0m;

        decimal dailyRate = baseSalary / standardWorkDays;
        return Math.Round(dailyRate * paidLeaveDays, 0, MidpointRounding.AwayFromZero);
    }

    public decimal ComputeOvertimePay(
        decimal baseSalary, 
        decimal standardWorkDays, 
        decimal overtimeHours, 
        decimal overtimeMultiplier = 1.5m)
    {
        if (baseSalary <= 0 || overtimeHours <= 0) return 0m;
        if (standardWorkDays <= 0) standardWorkDays = 22.0m;
        if (overtimeMultiplier <= 0) overtimeMultiplier = 1.5m;

        // Đơn giá 1 giờ chuẩn = Lương cơ bản / (Ngày công chuẩn * 8 giờ)
        decimal hourlyRate = baseSalary / (standardWorkDays * 8.0m);
        decimal overtimePay = hourlyRate * overtimeHours * overtimeMultiplier;
        return Math.Round(overtimePay, 0, MidpointRounding.AwayFromZero);
    }

    public decimal ComputeGrossSalary(
        decimal proratedBaseSalary, 
        decimal paidLeavePay, 
        decimal overtimePay, 
        decimal allowances, 
        decimal bonus = 0m)
    {
        decimal gross = proratedBaseSalary + paidLeavePay + overtimePay + allowances + bonus;
        return Math.Max(0m, Math.Round(gross, 0, MidpointRounding.AwayFromZero));
    }

    public Dictionary<string, decimal> ComputeMandatoryDeductions(decimal baseSalary, IEnumerable<DeductionRate>? rates)
    {
        var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        if (baseSalary <= 0 || rates == null) return result;

        foreach (var rate in rates.Where(r => r.IsActive))
        {
            decimal deductionAmount = rate.CalculateEmployeeDeduction(baseSalary);
            result[rate.RateCode.ToUpperInvariant()] = deductionAmount;
        }

        return result;
    }

    public decimal ComputePersonalIncomeTax(
        decimal grossSalary, 
        decimal nonTaxableAllowances, 
        decimal totalInsuranceDeductions, 
        decimal personalDeduction = VietnameseLaborRegulations.PersonalTaxDeduction)
    {
        // 1. Thu nhập chịu thuế = Gross - Các khoản phụ cấp không chịu thuế (ví dụ: ăn trưa tối đa 730k/tháng)
        decimal taxableIncome = Math.Max(0m, grossSalary - nonTaxableAllowances);

        // 2. Thu nhập tính thuế = Thu nhập chịu thuế - Các khoản giảm trừ (Bảo hiểm 10.5% + Giảm trừ bản thân mức mới 15tr)
        decimal incomeForTax = taxableIncome - totalInsuranceDeductions - personalDeduction;

        if (incomeForTax <= 0) return 0m;

        // 3. Biểu thuế lũy tiến từng phần theo Thông tư 111/2013/TT-BTC
        decimal tax = 0m;
        if (incomeForTax <= 5000000m)
        {
            tax = incomeForTax * 0.05m;
        }
        else if (incomeForTax <= 10000000m)
        {
            tax = (incomeForTax * 0.10m) - 250000m;
        }
        else if (incomeForTax <= 18000000m)
        {
            tax = (incomeForTax * 0.15m) - 750000m;
        }
        else if (incomeForTax <= 32000000m)
        {
            tax = (incomeForTax * 0.20m) - 1650000m;
        }
        else if (incomeForTax <= 52000000m)
        {
            tax = (incomeForTax * 0.25m) - 3250000m;
        }
        else if (incomeForTax <= 80000000m)
        {
            tax = (incomeForTax * 0.30m) - 5850000m;
        }
        else
        {
            tax = (incomeForTax * 0.35m) - 9850000m;
        }

        return Math.Max(0m, Math.Round(tax, 0, MidpointRounding.AwayFromZero));
    }

    public decimal ComputeNetSalary(decimal grossSalary, decimal totalDeductions)
    {
        decimal net = grossSalary - totalDeductions;
        return Math.Max(0m, Math.Round(net, 0, MidpointRounding.AwayFromZero));
    }
}
