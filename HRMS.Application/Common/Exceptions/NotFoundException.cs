namespace HRMS.Application.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested domain entity or resource is not found.
    /// </summary>
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message)
            : base(message, "RESOURCE.NOT_FOUND")
        {
        }

        public NotFoundException(string entityName, object key)
            : base($"Entity '{entityName}' with key '{key}' was not found.", "RESOURCE.NOT_FOUND")
        {
        }
    }
}
