using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.AdminPortal;

public interface ICalculatePayrollBatchUseCase
{
    Task<IEnumerable<Payslip>> ExecuteAsync(long periodId);
}

public class CalculatePayrollBatchUseCase : ICalculatePayrollBatchUseCase
{
    private readonly IEmployeeRepository _empRepo;
    private readonly ITimesheetRepository _timesheetRepo;
    private readonly IPayrollPeriodRepository _periodRepo;
    private readonly IDeductionRateRepository _rateRepo;
    private readonly IPayslipRepository _payslipRepo;
    private readonly IPayrollCalculationService _calcService;

    public CalculatePayrollBatchUseCase(
        IEmployeeRepository empRepo,
        ITimesheetRepository timesheetRepo,
        IPayrollPeriodRepository periodRepo,
        IDeductionRateRepository rateRepo,
        IPayslipRepository payslipRepo,
        IPayrollCalculationService calcService)
    {
        _empRepo = empRepo;
        _timesheetRepo = timesheetRepo;
        _periodRepo = periodRepo;
        _rateRepo = rateRepo;
        _payslipRepo = payslipRepo;
        _calcService = calcService;
    }

    public async Task<IEnumerable<Payslip>> ExecuteAsync(long periodId)
    {
        var period = await _periodRepo.GetPeriodByIdAsync(periodId);
        if (period == null) return Enumerable.Empty<Payslip>();

        var employees = await _empRepo.GetEmployeesAsync();
        var rates = (await _rateRepo.GetActiveRatesAsync()).ToList();

        var payslips = new List<Payslip>();
        foreach (var emp in employees.Where(e => e.Status == "ACTIVE"))
        {
            decimal actualDays = await _timesheetRepo.CountWorkDaysByEmployeeAndPeriodAsync(emp.Id, period.PeriodMonth, period.PeriodYear);
            var payslip = _calcService.CalculatePayslip(emp, periodId, actualDays, period.StandardWorkDays, rates);
            payslips.Add(payslip);
        }

        await _payslipRepo.SavePayslipBatchAsync(payslips);

        decimal totalGross = payslips.Sum(p => p.GrossSalary);
        decimal totalNet = payslips.Sum(p => p.NetSalary);
        await _periodRepo.UpdatePeriodStatusAsync(periodId, PeriodStatus.CALCULATED, totalGross, totalNet);

        return payslips;
    }
}

public interface IViewPayrollSummaryUseCase
{
    Task<(PayrollPeriod? Period, IEnumerable<Payslip> Payslips)> ExecuteAsync(long periodId);
}

public class ViewPayrollSummaryUseCase : IViewPayrollSummaryUseCase
{
    private readonly IPayrollPeriodRepository _periodRepo;
    private readonly IPayslipRepository _payslipRepo;

    public ViewPayrollSummaryUseCase(IPayrollPeriodRepository periodRepo, IPayslipRepository payslipRepo)
    {
        _periodRepo = periodRepo;
        _payslipRepo = payslipRepo;
    }

    public async Task<(PayrollPeriod? Period, IEnumerable<Payslip> Payslips)> ExecuteAsync(long periodId)
    {
        var period = await _periodRepo.GetPeriodByIdAsync(periodId);
        var payslips = await _payslipRepo.GetPayslipsByPeriodIdAsync(periodId);
        return (period, payslips);
    }
}

public interface ILockPayrollPeriodUseCase
{
    Task<bool> ExecuteAsync(long periodId, string lockedBy);
}

public class LockPayrollPeriodUseCase : ILockPayrollPeriodUseCase
{
    private readonly IPayrollPeriodRepository _periodRepo;
    private readonly ITimesheetRepository _timesheetRepo;

    public LockPayrollPeriodUseCase(IPayrollPeriodRepository periodRepo, ITimesheetRepository timesheetRepo)
    {
        _periodRepo = periodRepo;
        _timesheetRepo = timesheetRepo;
    }

    public async Task<bool> ExecuteAsync(long periodId, string lockedBy)
    {
        var period = await _periodRepo.GetPeriodByIdAsync(periodId);
        if (period == null) return false;

        // Khóa kỳ lương và khóa toàn bộ timesheet của kỳ sang chế độ Read-Only
        await _timesheetRepo.LockTimesheetsByPeriodAsync(periodId);
        await _periodRepo.UpdatePeriodStatusAsync(periodId, PeriodStatus.LOCKED, period.TotalGrossAmount, period.TotalNetAmount);
        return true;
    }
}

public interface IViewTimesheetSummaryUseCase
{
    Task<IEnumerable<Timesheet>> ExecuteAsync(long periodId);
}

public class ViewTimesheetSummaryUseCase : IViewTimesheetSummaryUseCase
{
    private readonly ITimesheetRepository _timesheetRepo;

    public ViewTimesheetSummaryUseCase(ITimesheetRepository timesheetRepo)
    {
        _timesheetRepo = timesheetRepo;
    }

    public async Task<IEnumerable<Timesheet>> ExecuteAsync(long periodId)
    {
        return await _timesheetRepo.GetTimesheetsByPeriodAsync(periodId);
    }
}

public interface IProcessLeaveRequestUseCase
{
    Task<IEnumerable<LeaveRequest>> GetPendingRequestsAsync();
    Task ApproveAsync(long requestId, string reviewer);
    Task RejectAsync(long requestId, string reviewer);
}

public class ProcessLeaveRequestUseCase : IProcessLeaveRequestUseCase
{
    private readonly ILeaveRequestRepository _leaveRepo;

    public ProcessLeaveRequestUseCase(ILeaveRequestRepository leaveRepo)
    {
        _leaveRepo = leaveRepo;
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingRequestsAsync()
    {
        return await _leaveRepo.GetPendingLeaveRequestsAsync();
    }

    public async Task ApproveAsync(long requestId, string reviewer)
    {
        await _leaveRepo.ApproveOrRejectLeaveRequestAsync(requestId, ApprovalStatus.APPROVED, reviewer);
    }

    public async Task RejectAsync(long requestId, string reviewer)
    {
        await _leaveRepo.ApproveOrRejectLeaveRequestAsync(requestId, ApprovalStatus.REJECTED, reviewer);
    }
}
