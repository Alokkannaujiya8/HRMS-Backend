namespace HRMS.Domain.Entities
{
    /// <summary>
    /// Entity tracking emergency contact information for an employee.
    /// </summary>
    public class EmergencyContact
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public string ContactName { get; set; } = string.Empty;

        public string Relationship { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? Address { get; set; }
    }
}
