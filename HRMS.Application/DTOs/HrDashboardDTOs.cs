namespace HRMS.Application.DTOs
{
    public class HrDashboardSummaryResponse
    {
        public int TotalEmployees { get; set; }

        public int TotalDepartments { get; set; }

        public int TotalPendingLeaves { get; set; }

        public int TotalPayrollRecords { get; set; }
    }
}
