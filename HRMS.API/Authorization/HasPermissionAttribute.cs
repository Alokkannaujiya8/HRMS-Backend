using Microsoft.AspNetCore.Authorization;

namespace HRMS.API.Authorization
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission)
        {
            Policy = $"Permission:{permission}";
        }
    }
}
