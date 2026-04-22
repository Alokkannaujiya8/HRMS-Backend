namespace HRMS.Domain.Entities
{
    public class SalaryManagement
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public decimal BasicSalary { get; set; }

        public decimal IncrementPercent { get; set; }

        public decimal IncrementAmount { get; set; }

        public DateTime EffectiveDate { get; set; }

        public decimal SpecialAllowance { get; set; }

        public decimal SpecialDeduction { get; set; }

        public decimal TotalSalary { get; set; }
    }
}
