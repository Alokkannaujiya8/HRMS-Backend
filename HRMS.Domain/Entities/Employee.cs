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

        public string? PhotoUrl { get; set; }

        public string? Address { get; set; }

        public string? Designation { get; set; }

        public string? Division { get; set; }

        public string? Pan { get; set; }

        public DateTime? Dob { get; set; }

        public string? Gender { get; set; }

        public string? EmploymentStatus { get; set; }

        public string? EmploymentType { get; set; }

        public bool IsSeventhPayCommission { get; set; }

        public string? BankAccountNumber { get; set; }

        public string? IfscCode { get; set; }

        public string? AadhaarNumber { get; set; }

        public bool IsAadhaarVerified { get; set; }

        public ICollection<EmployeeSkill>? Skills { get; set; }

        public ICollection<EmployeeRemark>? Remarks { get; set; }

        public ICollection<EmployeeDocument>? Documents { get; set; }
    }
}
