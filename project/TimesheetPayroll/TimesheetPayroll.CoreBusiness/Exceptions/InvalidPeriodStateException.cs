using TimesheetPayroll.CoreBusiness.Models;

namespace TimesheetPayroll.CoreBusiness.Exceptions;

/// <summary>
/// Ngoại lệ phát sinh khi thực hiện chuyển đổi trạng thái không hợp lệ trong vòng đời của Kỳ Lương.
/// </summary>
public class InvalidPeriodStateException : DomainException
{
    public PeriodStatus CurrentStatus { get; }
    public PeriodStatus AttemptedStatus { get; }

    public InvalidPeriodStateException(string message) : base(message)
    {
    }

    public InvalidPeriodStateException(string periodCode, PeriodStatus current, PeriodStatus attempted)
        : base($"Kỳ lương '{periodCode}' đang ở trạng thái '{current}' không thể chuyển sang '{attempted}'.")
    {
        CurrentStatus = current;
        AttemptedStatus = attempted;
    }
}
