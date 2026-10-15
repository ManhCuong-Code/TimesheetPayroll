using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.AdminPortal;

/// <summary>
/// Ca sử dụng tra cứu danh mục ca làm việc (Work Shifts).
/// </summary>
public interface IGetWorkShiftsUseCase
{
    Task<IEnumerable<WorkShift>> ExecuteAsync(bool onlyActive = false);
    Task<WorkShift?> GetByIdAsync(long id);
}

public class GetWorkShiftsUseCase : IGetWorkShiftsUseCase
{
    private readonly IWorkShiftRepository _shiftRepo;

    public GetWorkShiftsUseCase(IWorkShiftRepository shiftRepo)
    {
        _shiftRepo = shiftRepo;
    }

    public async Task<IEnumerable<WorkShift>> ExecuteAsync(bool onlyActive = false)
    {
        var shifts = await _shiftRepo.GetWorkShiftsAsync();
        return onlyActive ? shifts.Where(s => s.IsActive) : shifts;
    }

    public async Task<WorkShift?> GetByIdAsync(long id)
    {
        if (id <= 0) return null;
        return await _shiftRepo.GetWorkShiftByIdAsync(id);
    }
}

/// <summary>
/// Ca sử dụng quản lý (Thêm/Sửa/Xóa/Bật-Tắt) ca làm việc trong doanh nghiệp.
/// </summary>
public interface IManageWorkShiftUseCase
{
    Task<long> CreateAsync(WorkShift shift);
    Task UpdateAsync(WorkShift shift);
    Task DeleteAsync(long id);
    Task ToggleStatusAsync(long id);
}

public class ManageWorkShiftUseCase : IManageWorkShiftUseCase
{
    private readonly IWorkShiftRepository _shiftRepo;

    public ManageWorkShiftUseCase(IWorkShiftRepository shiftRepo)
    {
        _shiftRepo = shiftRepo;
    }

    public async Task<long> CreateAsync(WorkShift shift)
    {
        ArgumentNullException.ThrowIfNull(shift);
        if (string.IsNullOrWhiteSpace(shift.ShiftCode))
            throw new ArgumentException("Mã ca làm việc không được để trống.", nameof(shift.ShiftCode));
        if (string.IsNullOrWhiteSpace(shift.ShiftName))
            throw new ArgumentException("Tên ca làm việc không được để trống.", nameof(shift.ShiftName));

        var existing = await _shiftRepo.GetWorkShiftByCodeAsync(shift.ShiftCode.Trim());
        if (existing != null)
            throw new InvalidOperationException($"Mã ca làm việc '{shift.ShiftCode}' đã tồn tại trong hệ thống.");

        return await _shiftRepo.AddWorkShiftAsync(shift);
    }

    public async Task UpdateAsync(WorkShift shift)
    {
        ArgumentNullException.ThrowIfNull(shift);
        if (shift.Id <= 0)
            throw new ArgumentException("ID ca làm việc không hợp lệ.", nameof(shift.Id));
        if (string.IsNullOrWhiteSpace(shift.ShiftCode))
            throw new ArgumentException("Mã ca làm việc không được để trống.", nameof(shift.ShiftCode));
        if (string.IsNullOrWhiteSpace(shift.ShiftName))
            throw new ArgumentException("Tên ca làm việc không được để trống.", nameof(shift.ShiftName));

        await _shiftRepo.UpdateWorkShiftAsync(shift);
    }

    public async Task DeleteAsync(long id)
    {
        if (id <= 0) return;
        await _shiftRepo.DeleteWorkShiftAsync(id);
    }

    public async Task ToggleStatusAsync(long id)
    {
        if (id <= 0) return;
        var shift = await _shiftRepo.GetWorkShiftByIdAsync(id);
        if (shift != null)
        {
            shift.IsActive = !shift.IsActive;
            await _shiftRepo.UpdateWorkShiftAsync(shift);
        }
    }
}
