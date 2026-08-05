namespace HRMS.Application.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when a business rule or domain invariant is violated.
    /// </summary>
    public class BusinessException : BaseException
    {
        public BusinessException(string message, string errorCode = "BUSINESS.RULE_VIOLATION")
            : base(message, errorCode)
        {
        }
    }
}
