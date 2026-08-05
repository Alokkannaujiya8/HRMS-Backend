namespace HRMS.Domain.Entities
{
    /// <summary>
    /// Entity tracking prior employment experience and company background.
    /// </summary>
    public class EmployeeExperience
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string Designation { get; set; } = string.Empty;

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public decimal? LastSalary { get; set; }

        public string? ReasonForLeaving { get; set; }
    }
}
