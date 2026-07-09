using System;

namespace HRMS.Domain.Entities
{
    public class AssetAssignment
    {
        public int Id { get; set; }

        public int AssetId { get; set; }

        public Asset? Asset { get; set; }

        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ReturnedDate { get; set; }

        public string? ConditionOnAssign { get; set; }

        public string? ConditionOnReturn { get; set; }

        public string? Notes { get; set; }
    }
}
