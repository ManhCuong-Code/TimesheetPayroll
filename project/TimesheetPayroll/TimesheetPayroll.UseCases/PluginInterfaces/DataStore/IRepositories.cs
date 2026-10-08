using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(long id);
    Task<Employee?> GetEmployeeByCodeAsync(string code);
}

public interface ITimesheetRepository
{
    Task<IEnumerable<Timesheet>> GetTimesheetsByPeriodAsync(long periodId);
    Task<IEnumerable<Timesheet>> GetTimesheetsByEmployeeAndMonthAsync(long employeeId, int month, int year);
    Task<decimal> CountWorkDaysByEmployeeAndPeriodAsync(long employeeId, int month, int year);
    Task AddOrUpdateTimesheetAsync(Timesheet timesheet);
    Task LockTimesheetsByPeriodAsync(long periodId);
}

public interface IPayrollPeriodRepository
{
    Task<IEnumerable<PayrollPeriod>> GetPeriodsAsync();
    Task<PayrollPeriod?> GetPeriodByIdAsync(long id);
    Task<PayrollPeriod?> GetPeriodByMonthYearAsync(int month, int year);
    Task UpdatePeriodStatusAsync(long periodId, PeriodStatus status, decimal grossTotal, decimal netTotal);
}

public interface IPayslipRepository
{
    Task<IEnumerable<Payslip>> GetPayslipsByPeriodIdAsync(long periodId);
    Task<Payslip?> GetPayslipByPeriodAndEmployeeAsync(long periodId, long employeeId);
    Task SavePayslipBatchAsync(IEnumerable<Payslip> payslips);
}

public interface IDeductionRateRepository
{
    Task<IEnumerable<DeductionRate>> GetActiveRatesAsync();
}

public interface ILeaveRequestRepository
{
    Task<IEnumerable<LeaveRequest>> GetPendingLeaveRequestsAsync();
    Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeAsync(long employeeId);
    Task CreateLeaveRequestAsync(LeaveRequest request);
    Task ApproveOrRejectLeaveRequestAsync(long requestId, ApprovalStatus status, string reviewer);
}
