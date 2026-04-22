namespace HRMS.Domain.Entities
{
    public class SalaryStructure
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public decimal Base { get; set; }

        public decimal HRA { get; set; }

        public decimal DA { get; set; }

        public decimal PFDeductions { get; set; }

        public decimal Tax { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
