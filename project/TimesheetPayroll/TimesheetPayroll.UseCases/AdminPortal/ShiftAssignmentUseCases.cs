using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.AdminPortal;

public interface IGetShiftAssignmentsUseCase
{
    Task<IEnumerable<ShiftAssignment>> ExecuteByDateAsync(DateTime date);
    Task<IEnumerable<ShiftAssignment>> ExecuteByMonthAsync(int month, int year);
    Task<IEnumerable<ShiftAssignment>> ExecuteByEmployeeAsync(long employeeId, DateTime start, DateTime end);
}

public class GetShiftAssignmentsUseCase : IGetShiftAssignmentsUseCase
{
    private readonly IShiftAssignmentRepository _repo;
    public GetShiftAssignmentsUseCase(IShiftAssignmentRepository repo) => _repo = repo;

    public async Task<IEnumerable<ShiftAssignment>> ExecuteByDateAsync(DateTime date) =>
        await _repo.GetAssignmentsByDateAsync(date);

    public async Task<IEnumerable<ShiftAssignment>> ExecuteByMonthAsync(int month, int year) =>
        await _repo.GetAssignmentsByMonthAsync(month, year);

    public async Task<IEnumerable<ShiftAssignment>> ExecuteByEmployeeAsync(long employeeId, DateTime start, DateTime end) =>
        await _repo.GetAssignmentsByEmployeeAsync(employeeId, start, end);
}

public interface IManageShiftAssignmentUseCase
{
    Task<long> AssignAsync(ShiftAssignment assignment);
    Task AssignBatchAsync(IEnumerable<ShiftAssignment> assignments);
    Task DeleteAsync(long id);
}

public class ManageShiftAssignmentUseCase : IManageShiftAssignmentUseCase
{
    private readonly IShiftAssignmentRepository _repo;
    public ManageShiftAssignmentUseCase(IShiftAssignmentRepository repo) => _repo = repo;

    public async Task<long> AssignAsync(ShiftAssignment assignment) =>
        await _repo.AssignShiftAsync(assignment);

    public async Task AssignBatchAsync(IEnumerable<ShiftAssignment> assignments) =>
        await _repo.AssignShiftBatchAsync(assignments);

    public async Task DeleteAsync(long id) =>
        await _repo.DeleteAssignmentAsync(id);
}
