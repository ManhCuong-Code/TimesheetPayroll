using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.DataStore.HandCoded;

public class HandCodedRepositories : IEmployeeRepository, ITimesheetRepository, IPayrollPeriodRepository, IPayslipRepository, IDeductionRateRepository, ILeaveRequestRepository
{
    private readonly List<Employee> _employees = new()
    {
        new Employee { Id = 1, EmployeeCode = "EMP001", FullName = "Nguyễn Văn A", Email = "nguyenvana@congty.com", DepartmentName = "Phòng Công Nghệ Thông Tin", BaseSalary = 10000000m, MealAllowance = 500000m, BankAccountNumber = "19035678901234", BankName = "Vietcombank" },
        new Employee { Id = 2, EmployeeCode = "EMP002", FullName = "Lê Thị Thu Thảo", Email = "thaolt@congty.com", DepartmentName = "Phòng Hành Chính Nhân Sự", BaseSalary = 15000000m, MealAllowance = 500000m, FuelAllowance = 500000m, BankAccountNumber = "00210003456789", BankName = "MB Bank" },
        new Employee { Id = 3, EmployeeCode = "EMP003", FullName = "Trần Đình Trọng", Email = "trongtd@congty.com", DepartmentName = "Ban Giám Đốc", BaseSalary = 25000000m, MealAllowance = 500000m, FuelAllowance = 1000000m, BankAccountNumber = "10123498765432", BankName = "Techcombank" }
    };

    private readonly List<PayrollPeriod> _periods = new()
    {
        new PayrollPeriod { Id = 10, PeriodCode = "PR-2026-10", PeriodMonth = 10, PeriodYear = 2026, StandardWorkDays = 22.0m, Status = PeriodStatus.CALCULATED }
    };

    private readonly List<Timesheet> _timesheets = new();
    private readonly List<Payslip> _payslips = new();
    private readonly List<LeaveRequest> _leaveRequests = new();

    private readonly List<DeductionRate> _rates = new()
    {
        new DeductionRate { Id = 1, RateCode = "BHXH", RateName = "Bảo hiểm Xã hội", EmployeeRate = 0.0800m, EmployerRate = 0.1750m },
        new DeductionRate { Id = 2, RateCode = "BHYT", RateName = "Bảo hiểm Y tế", EmployeeRate = 0.0150m, EmployerRate = 0.0300m },
        new DeductionRate { Id = 3, RateCode = "BHTN", RateName = "Bảo hiểm Thất nghiệp", EmployeeRate = 0.0100m, EmployerRate = 0.0100m }
    };

    public Task<IEnumerable<Employee>> GetEmployeesAsync() => Task.FromResult<IEnumerable<Employee>>(_employees);
    public Task<Employee?> GetEmployeeByIdAsync(long id) => Task.FromResult(_employees.FirstOrDefault(e => e.Id == id));
    public Task<Employee?> GetEmployeeByCodeAsync(string code) => Task.FromResult(_employees.FirstOrDefault(e => e.EmployeeCode == code));

    public Task<IEnumerable<Timesheet>> GetTimesheetsByPeriodAsync(long periodId) => Task.FromResult<IEnumerable<Timesheet>>(_timesheets.Where(t => t.PeriodId == periodId));
    public Task<IEnumerable<Timesheet>> GetTimesheetsByEmployeeAndMonthAsync(long employeeId, int month, int year) =>
        Task.FromResult<IEnumerable<Timesheet>>(_timesheets.Where(t => t.EmployeeId == employeeId && t.WorkDate.Month == month && t.WorkDate.Year == year));
    public Task<decimal> CountWorkDaysByEmployeeAndPeriodAsync(long employeeId, int month, int year)
    {
        var sum = _timesheets.Where(t => t.EmployeeId == employeeId && t.WorkDate.Month == month && t.WorkDate.Year == year).Sum(t => t.WorkUnits);
        return Task.FromResult(sum > 0 ? sum : 20.0m);
    }
    public Task AddOrUpdateTimesheetAsync(Timesheet t)
    {
        var existing = _timesheets.FirstOrDefault(x => x.EmployeeId == t.EmployeeId && x.WorkDate.Date == t.WorkDate.Date);
        if (existing != null) _timesheets.Remove(existing);
        _timesheets.Add(t);
        return Task.CompletedTask;
    }
    public Task LockTimesheetsByPeriodAsync(long periodId)
    {
        foreach (var t in _timesheets.Where(x => x.PeriodId == periodId)) t.IsLocked = true;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<PayrollPeriod>> GetPeriodsAsync() => Task.FromResult<IEnumerable<PayrollPeriod>>(_periods);
    public Task<PayrollPeriod?> GetPeriodByIdAsync(long id) => Task.FromResult(_periods.FirstOrDefault(p => p.Id == id));
    public Task<PayrollPeriod?> GetPeriodByMonthYearAsync(int month, int year) => Task.FromResult(_periods.FirstOrDefault(p => p.PeriodMonth == month && p.PeriodYear == year));
    public Task UpdatePeriodStatusAsync(long periodId, PeriodStatus status, decimal grossTotal, decimal netTotal)
    {
        var p = _periods.FirstOrDefault(x => x.Id == periodId);
        if (p != null) { p.Status = status; p.TotalGrossAmount = grossTotal; p.TotalNetAmount = netTotal; }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Payslip>> GetPayslipsByPeriodIdAsync(long periodId) => Task.FromResult<IEnumerable<Payslip>>(_payslips.Where(p => p.PeriodId == periodId));
    public Task<Payslip?> GetPayslipByPeriodAndEmployeeAsync(long periodId, long employeeId) =>
        Task.FromResult(_payslips.FirstOrDefault(p => p.PeriodId == periodId && p.EmployeeId == employeeId));
    public Task SavePayslipBatchAsync(IEnumerable<Payslip> payslips)
    {
        foreach (var p in payslips)
        {
            var old = _payslips.FirstOrDefault(x => x.PeriodId == p.PeriodId && x.EmployeeId == p.EmployeeId);
            if (old != null) _payslips.Remove(old);
            _payslips.Add(p);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<DeductionRate>> GetActiveRatesAsync() => Task.FromResult<IEnumerable<DeductionRate>>(_rates);

    public Task<IEnumerable<LeaveRequest>> GetPendingLeaveRequestsAsync() => Task.FromResult<IEnumerable<LeaveRequest>>(_leaveRequests.Where(r => r.ApprovalStatus == ApprovalStatus.PENDING));
    public Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeAsync(long employeeId) => Task.FromResult<IEnumerable<LeaveRequest>>(_leaveRequests.Where(r => r.EmployeeId == employeeId));
    public Task CreateLeaveRequestAsync(LeaveRequest request) { _leaveRequests.Add(request); return Task.CompletedTask; }
    public Task ApproveOrRejectLeaveRequestAsync(long requestId, ApprovalStatus status, string reviewer)
    {
        var req = _leaveRequests.FirstOrDefault(r => r.Id == requestId);
        if (req != null) { req.ApprovalStatus = status; req.ApprovedBy = reviewer; req.ApprovedAt = DateTime.Now; }
        return Task.CompletedTask;
    }
}
