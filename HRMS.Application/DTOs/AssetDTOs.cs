using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Application.DTOs
{
    public class AssetDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? AssetTag { get; set; }
        public string? Category { get; set; }
        public string? SerialNumber { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? Value { get; set; }
        
        // Active assignment details
        public int? AssignedToEmployeeId { get; set; }
        public string? AssignedToEmployeeName { get; set; }
        public DateTime? AssignedDate { get; set; }
    }

    public class AssetDetailDto : AssetDto
    {
        public List<AssetAssignmentDto> AssignmentHistory { get; set; } = new();
    }

    public class AssetCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string AssetTag { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        public string? SerialNumber { get; set; }
        public string? Description { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? Value { get; set; }
    }

    public class AssetUpdateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string AssetTag { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        public string? SerialNumber { get; set; }
        public string? Status { get; set; } // Available, UnderRepair, Retired
        public string? Description { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? Value { get; set; }
    }

    public class AssetAssignDto
    {
        [Required]
        public int EmployeeId { get; set; }

        public string? ConditionOnAssign { get; set; }

        public string? Notes { get; set; }
    }

    public class AssetReturnDto
    {
        public string? ConditionOnReturn { get; set; }

        public string? Notes { get; set; }
    }

    public class AssetAssignmentDto
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string? AssetName { get; set; }
        public string? AssetTag { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public string? ConditionOnAssign { get; set; }
        public string? ConditionOnReturn { get; set; }
        public string? Notes { get; set; }
    }
}
