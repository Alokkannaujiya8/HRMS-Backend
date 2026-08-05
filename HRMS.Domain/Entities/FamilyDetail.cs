namespace HRMS.Domain.Entities
{
    /// <summary>
    /// Entity tracking family members and dependent details for an employee.
    /// </summary>
    public class FamilyDetail
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public string Relationship { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        public bool IsDependent { get; set; }
    }
}
