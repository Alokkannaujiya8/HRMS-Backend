using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Domain.Entities
{
    public class LeaveBalance
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public string LeaveType { get; set; } = "Casual";

        public int TotalLeaves { get; set; } = 20;

        public int UsedLeaves { get; set; } = 0;

        [NotMapped]
        public int RemainingLeaves => TotalLeaves - UsedLeaves;
    }
}
