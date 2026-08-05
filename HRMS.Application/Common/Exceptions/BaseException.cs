namespace HRMS.Application.Common.Exceptions
{
    /// <summary>
    /// Base exception class for all custom domain and application exceptions in HRMS.
    /// </summary>
    public abstract class BaseException : Exception
    {
        public string ErrorCode { get; }

        protected BaseException(string message, string errorCode = "ERROR.GENERAL")
            : base(message)
        {
            ErrorCode = errorCode;
        }

        protected BaseException(string message, Exception innerException, string errorCode = "ERROR.GENERAL")
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
