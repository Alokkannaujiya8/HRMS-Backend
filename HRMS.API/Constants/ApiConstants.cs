namespace HRMS.API.Constants
{
    public static class ApiConstants
    {
        public const string BearerAuthenticationScheme = "Bearer";
        public const string AllowAngularAppCorsPolicy = "AllowAngularApp";
        public const string DefaultHangfireTimeZoneId = "India Standard Time";
    }

    public static class HangfireJobIds
    {
        public const string MarkAbsentNightly = "mark-absent-nightly";
        public const string RecalculateMonthlyLeaveBalances = "recalculate-monthly-leave-balances";
    }
}
