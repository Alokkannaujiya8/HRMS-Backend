namespace HRMS.Application.Security
{
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Hr = "HR";
        public const string Employee = "Employee";
        public const string AdminOrHr = Admin + "," + Hr;

        public const string NormalizedAdmin = "ADMIN";
        public const string NormalizedHr = "HR";
        public const string NormalizedEmployee = "EMPLOYEE";
    }
}
