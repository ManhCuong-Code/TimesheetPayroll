namespace TimesheetPayroll.CoreBusiness.Exceptions;

/// <summary>
/// Ngoại lệ phát sinh khi thông tin hoặc thao tác duyệt đơn xin nghỉ phép không hợp lệ.
/// </summary>
public class LeaveRequestException : DomainException
{
    public LeaveRequestException(string message) : base(message)
    {
    }
}
