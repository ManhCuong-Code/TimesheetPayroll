using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.CoreBusiness.Services.Interfaces;

public interface IPayrollCalculationService
{
    Payslip CalculatePayslip(Employee employee, long periodId, decimal actualWorkDays, decimal standardWorkDays, List<DeductionRate> rates);
}
