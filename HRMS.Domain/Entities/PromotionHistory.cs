namespace HRMS.Domain.Entities
{
    /// <summary>
    /// Entity tracking internal promotions, role upgrades, and salary revisions over time.
    /// </summary>
    public class PromotionHistory
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public string PreviousDesignation { get; set; } = string.Empty;

        public string NewDesignation { get; set; } = string.Empty;

        public decimal PreviousSalary { get; set; }

        public decimal NewSalary { get; set; }

        public DateTime PromotionDate { get; set; } = DateTime.UtcNow;

        public string? ApprovedBy { get; set; }
    }
}
