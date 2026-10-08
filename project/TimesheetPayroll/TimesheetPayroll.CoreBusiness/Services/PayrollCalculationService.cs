using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;

namespace TimesheetPayroll.CoreBusiness.Services;

public class PayrollCalculationService : IPayrollCalculationService
{
    public Payslip CalculatePayslip(Employee employee, long periodId, decimal actualWorkDays, decimal standardWorkDays, List<DeductionRate> rates)
    {
        if (standardWorkDays <= 0) standardWorkDays = 22.0m;

        // 1. Lương ngày công thực tế theo tỷ lệ
        decimal baseSalaryProrated = Math.Round(employee.BaseSalary * (actualWorkDays / standardWorkDays), 0, MidpointRounding.AwayFromZero);

        // 2. Thu nhập gộp (Gross)
        decimal grossSalary = baseSalaryProrated + employee.MealAllowance + employee.FuelAllowance;

        // 3. Khấu trừ bảo hiểm xã hội, y tế, thất nghiệp
        decimal totalDeductions = 0m;
        var lines = new List<PayslipLine>
        {
            new PayslipLine
            {
                LineType = PayslipLineType.BASE_SALARY_PRORATED,
                LineDescription = $"Lương ngày công thực tế ({actualWorkDays}/{standardWorkDays} ngày)",
                Amount = baseSalaryProrated,
                IsDeduction = false
            }
        };

        if (employee.MealAllowance > 0)
        {
            lines.Add(new PayslipLine
            {
                LineType = PayslipLineType.MEAL_ALLOWANCE,
                LineDescription = "Phụ cấp tiền ăn trưa",
                Amount = employee.MealAllowance,
                IsDeduction = false
            });
        }

        if (employee.FuelAllowance > 0)
        {
            lines.Add(new PayslipLine
            {
                LineType = PayslipLineType.FUEL_ALLOWANCE,
                LineDescription = "Phụ cấp xăng xe / đi lại",
                Amount = employee.FuelAllowance,
                IsDeduction = false
            });
        }

        foreach (var rate in rates.Where(r => r.IsActive))
        {
            // Trích nộp bảo hiểm tính trên lương cơ bản
            decimal deductionAmount = Math.Round(employee.BaseSalary * rate.EmployeeRate, 0, MidpointRounding.AwayFromZero);
            totalDeductions += deductionAmount;

            var lineType = rate.RateCode.ToUpperInvariant() switch
            {
                "BHXH" => PayslipLineType.DEDUCTION_BHXH,
                "BHYT" => PayslipLineType.DEDUCTION_BHYT,
                "BHTN" => PayslipLineType.DEDUCTION_BHTN,
                _ => PayslipLineType.PENALTY
            };

            lines.Add(new PayslipLine
            {
                LineType = lineType,
                LineDescription = $"Trích nộp {rate.RateName} ({rate.EmployeeRate * 100:0.#}%)",
                Amount = deductionAmount,
                IsDeduction = true
            });
        }

        // 4. Thực lĩnh (Net)
        decimal netSalary = Math.Max(0, grossSalary - totalDeductions);

        return new Payslip
        {
            PeriodId = periodId,
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = employee.FullName,
            DepartmentName = employee.DepartmentName,
            BankAccountNumber = employee.BankAccountNumber,
            BankName = employee.BankName,
            ActualWorkDays = actualWorkDays,
            GrossSalary = grossSalary,
            TotalDeductions = totalDeductions,
            NetSalary = netSalary,
            Lines = lines
        };
    }
}
