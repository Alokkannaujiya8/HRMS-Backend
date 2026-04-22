namespace HRMS.Application.Security
{
    public static class RolePermissionStore
    {
        public static IReadOnlyCollection<string> GetPermissionsForRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return Array.Empty<string>();
            }

            return role.Trim().ToUpperInvariant() switch
            {
                "ADMIN" => new[]
                {
                    PermissionConstants.CanViewSalary,
                    PermissionConstants.CanEditAttendance,
                    PermissionConstants.CanManageLeaves,
                    PermissionConstants.CanManageDepartments,
                    PermissionConstants.CanManageUsers
                },
                "HR" => new[]
                {
                    PermissionConstants.CanViewSalary,
                    PermissionConstants.CanEditAttendance,
                    PermissionConstants.CanManageLeaves,
                    PermissionConstants.CanManageDepartments,
                    PermissionConstants.CanManageUsers
                },
                "EMPLOYEE" => new[]
                {
                    PermissionConstants.CanEditAttendance
                },
                _ => Array.Empty<string>()
            };
        }
    }
}
