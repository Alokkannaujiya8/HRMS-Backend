using System;

namespace HRMS.Domain.Entities
{
    public class Asset
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? AssetTag { get; set; }

        public string? Category { get; set; }

        public string? SerialNumber { get; set; }

        public string? Status { get; set; } = "Available"; // Available, Assigned, UnderRepair, Retired

        public string? Description { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public decimal? Value { get; set; }
    }
}
