namespace TimesheetPayroll.CoreBusiness.Exceptions;

/// <summary>
/// Ngoại lệ phát sinh khi cố tình chỉnh sửa bản ghi chấm công đã bị khóa (Read-Only).
/// </summary>
public class TimesheetLockedException : DomainException
{
    public long TimesheetId { get; }
    public DateTime WorkDate { get; }

    public TimesheetLockedException(long timesheetId, DateTime workDate)
        : base($"Bản ghi chấm công ID={timesheetId} ngày {workDate:dd/MM/yyyy} đã bị khóa, không được phép thay đổi.")
    {
        TimesheetId = timesheetId;
        WorkDate = workDate;
    }
}
