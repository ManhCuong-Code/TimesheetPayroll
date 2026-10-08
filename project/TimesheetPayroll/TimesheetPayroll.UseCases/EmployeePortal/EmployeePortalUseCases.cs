using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.EmployeePortal;

public interface IViewMyPayslipUseCase
{
    Task<Payslip?> ExecuteAsync(long periodId, long employeeId);
}

public class ViewMyPayslipUseCase : IViewMyPayslipUseCase
{
    private readonly IPayslipRepository _payslipRepo;

    public ViewMyPayslipUseCase(IPayslipRepository payslipRepo)
    {
        _payslipRepo = payslipRepo;
    }

    public async Task<Payslip?> ExecuteAsync(long periodId, long employeeId)
    {
        return await _payslipRepo.GetPayslipByPeriodAndEmployeeAsync(periodId, employeeId);
    }
}

public interface IViewMyTimesheetUseCase
{
    Task<IEnumerable<Timesheet>> ExecuteAsync(long employeeId, int month, int year);
}

public class ViewMyTimesheetUseCase : IViewMyTimesheetUseCase
{
    private readonly ITimesheetRepository _timesheetRepo;

    public ViewMyTimesheetUseCase(ITimesheetRepository timesheetRepo)
    {
        _timesheetRepo = timesheetRepo;
    }

    public async Task<IEnumerable<Timesheet>> ExecuteAsync(long employeeId, int month, int year)
    {
        return await _timesheetRepo.GetTimesheetsByEmployeeAndMonthAsync(employeeId, month, year);
    }
}

public interface ICreateLeaveRequestUseCase
{
    Task ExecuteAsync(LeaveRequest request);
}

public class CreateLeaveRequestUseCase : ICreateLeaveRequestUseCase
{
    private readonly ILeaveRequestRepository _leaveRepo;

    public CreateLeaveRequestUseCase(ILeaveRequestRepository leaveRepo)
    {
        _leaveRepo = leaveRepo;
    }

    public async Task ExecuteAsync(LeaveRequest request)
    {
        await _leaveRepo.CreateLeaveRequestAsync(request);
    }
}

public interface ICheckInCheckOutUseCase
{
    Task<Timesheet> ExecuteCheckInAsync(long employeeId);
    Task<Timesheet> ExecuteCheckOutAsync(long employeeId);
}

public class CheckInCheckOutUseCase : ICheckInCheckOutUseCase
{
    private readonly ITimesheetRepository _timesheetRepo;

    public CheckInCheckOutUseCase(ITimesheetRepository timesheetRepo)
    {
        _timesheetRepo = timesheetRepo;
    }

    public async Task<Timesheet> ExecuteCheckInAsync(long employeeId)
    {
        var today = DateTime.Today;
        var existing = (await _timesheetRepo.GetTimesheetsByEmployeeAndMonthAsync(employeeId, today.Month, today.Year))
            .FirstOrDefault(t => t.WorkDate.Date == today);

        var timesheet = existing ?? new Timesheet
        {
            EmployeeId = employeeId,
            WorkDate = today,
            WorkUnits = 1.0m,
            WorkShiftStatus = WorkShiftStatus.PRESENT
        };

        timesheet.CheckInTime = DateTime.Now.TimeOfDay;
        await _timesheetRepo.AddOrUpdateTimesheetAsync(timesheet);
        return timesheet;
    }

    public async Task<Timesheet> ExecuteCheckOutAsync(long employeeId)
    {
        var today = DateTime.Today;
        var existing = (await _timesheetRepo.GetTimesheetsByEmployeeAndMonthAsync(employeeId, today.Month, today.Year))
            .FirstOrDefault(t => t.WorkDate.Date == today);

        var timesheet = existing ?? new Timesheet
        {
            EmployeeId = employeeId,
            WorkDate = today,
            WorkUnits = 1.0m,
            WorkShiftStatus = WorkShiftStatus.PRESENT,
            CheckInTime = new TimeSpan(8, 0, 0)
        };

        timesheet.CheckOutTime = DateTime.Now.TimeOfDay;
        if (timesheet.CheckInTime.HasValue)
        {
            var hours = (timesheet.CheckOutTime.Value - timesheet.CheckInTime.Value).TotalHours;
            timesheet.ActualHoursWorked = Math.Max(0, Math.Round((decimal)hours, 2));
        }

        await _timesheetRepo.AddOrUpdateTimesheetAsync(timesheet);
        return timesheet;
    }
}
