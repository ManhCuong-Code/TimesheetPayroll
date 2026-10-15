namespace TimesheetPayroll.CoreBusiness.Exceptions;

/// <summary>
/// Ngoại lệ phát sinh khi vi phạm quy tắc logic nghiệp vụ (Invariants) của thực thể miền.
/// </summary>
public class BusinessRuleValidationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleValidationException(string message, string ruleName = "GenericBusinessRule") 
        : base(message)
    {
        RuleName = ruleName;
    }
}
