namespace HRMS.Domain.Entities
{
    /// <summary>
    /// Entity tracking employee educational qualifications and degrees.
    /// </summary>
    public class EmployeeEducation
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public string Degree { get; set; } = string.Empty;

        public string Institution { get; set; } = string.Empty;

        public string? FieldOfStudy { get; set; }

        public int PassingYear { get; set; }

        public decimal? PercentageGrade { get; set; }
    }
}
