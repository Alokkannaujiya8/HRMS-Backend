namespace HRMS.Application.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when authentication fails or user token is invalid/missing.
    /// </summary>
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message = "Unauthorized access attempt.")
            : base(message, "AUTH.UNAUTHORIZED")
        {
        }
    }
}
