namespace TimesheetPayroll.CoreBusiness.Exceptions;

/// <summary>
/// Ngoại lệ nền tảng cho các lỗi nghiệp vụ trong tầng Domain CoreBusiness.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
