namespace HRMS.Domain.Entities
{
    public class Employee
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? Mobile { get; set; }

        public decimal Salary { get; set; }

        public int DepartmentId { get; set; }

        public Department? Department { get; set; }

        public DateTime JoinDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}