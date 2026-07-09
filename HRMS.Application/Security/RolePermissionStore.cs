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
                AppRoles.NormalizedAdmin => new[]
                {
                    PermissionConstants.CanViewSalary,
                    PermissionConstants.CanEditAttendance,
                    PermissionConstants.CanManageLeaves,
                    PermissionConstants.CanManageDepartments,
                    PermissionConstants.CanManageUsers
                },
                AppRoles.NormalizedHr => new[]
                {
                    PermissionConstants.CanViewSalary,
                    PermissionConstants.CanEditAttendance,
                    PermissionConstants.CanManageLeaves,
                    PermissionConstants.CanManageDepartments,
                    PermissionConstants.CanManageUsers
                },
                AppRoles.NormalizedEmployee => new[]
                {
                    PermissionConstants.CanEditAttendance
                },
                _ => Array.Empty<string>()
            };
        }
    }
}
