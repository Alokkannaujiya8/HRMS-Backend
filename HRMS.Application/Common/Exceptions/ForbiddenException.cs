namespace HRMS.Application.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when an authenticated user lacks required permissions or roles to execute an action.
    /// </summary>
    public class ForbiddenException : BaseException
    {
        public ForbiddenException(string message = "You do not have permission to perform this operation.")
            : base(message, "AUTH.FORBIDDEN")
        {
        }
    }
}
