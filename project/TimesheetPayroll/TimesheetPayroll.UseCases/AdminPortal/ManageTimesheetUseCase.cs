using System.Text.Json;
using TimesheetPayroll.CoreBusiness.Exceptions;
using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.UseCases.AdminPortal;

public interface IManageTimesheetUseCase
{
    Task AdjustTimesheetAsync(Timesheet timesheet, string reason, string callerUsername, string? callerRole);
    Task<IEnumerable<TimesheetAuditLog>> GetAuditLogsAsync(long? employeeId, string? callerRole);
}

public class ManageTimesheetUseCase : IManageTimesheetUseCase
{
    private readonly ITimesheetRepository _timesheetRepo;
    private readonly ITimesheetAuditLogRepository _auditRepo;

    public ManageTimesheetUseCase(ITimesheetRepository timesheetRepo, ITimesheetAuditLogRepository auditRepo)
    {
        _timesheetRepo = timesheetRepo;
        _auditRepo = auditRepo;
    }

    public async Task AdjustTimesheetAsync(Timesheet timesheet, string reason, string callerUsername, string? callerRole)
    {
        if (callerRole != "ROLE_ADMIN" && callerRole != "ROLE_HR_ACCOUNTANT" && callerRole != "ROLE_MANAGER")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền điều chỉnh bản ghi chấm công. Chỉ Quản lý, Kế toán và Admin mới được phép thực hiện.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Bắt buộc phải nhập lý do điều chỉnh bản ghi chấm công để phục vụ kiểm toán.", nameof(reason));
        }

        // Lấy dữ liệu trước khi sửa để ghi log
        var existingList = await _timesheetRepo.GetTimesheetsByEmployeeAndMonthAsync(timesheet.EmployeeId, timesheet.WorkDate.Month, timesheet.WorkDate.Year);
        var existing = existingList.FirstOrDefault(t => t.WorkDate.Date == timesheet.WorkDate.Date);

        if (existing?.IsLocked == true && callerRole != "ROLE_ADMIN")
        {
            throw new TimesheetLockedException(existing.Id, existing.WorkDate);
        }

        string beforeJson = existing != null ? JsonSerializer.Serialize(existing) : "Chưa có bản ghi cũ (Thêm mới)";

        // Cập nhật bản ghi
        await _timesheetRepo.AddOrUpdateTimesheetAsync(timesheet);

        string afterJson = JsonSerializer.Serialize(timesheet);

        // Ghi log kiểm toán
        await _auditRepo.LogAuditAsync(new TimesheetAuditLog
        {
            TimesheetId = timesheet.Id,
            EmployeeId = timesheet.EmployeeId,
            Action = existing != null ? "MANUAL_ADJUST" : "MANUAL_CREATE",
            BeforeData = beforeJson,
            AfterData = afterJson,
            ModifiedBy = $"{callerUsername} ({callerRole})",
            Reason = reason
        });
    }

    public async Task<IEnumerable<TimesheetAuditLog>> GetAuditLogsAsync(long? employeeId, string? callerRole)
    {
        if (employeeId.HasValue && employeeId.Value > 0)
        {
            return await _auditRepo.GetLogsByEmployeeAsync(employeeId.Value);
        }
        return await _auditRepo.GetRecentLogsAsync(100);
    }
}
