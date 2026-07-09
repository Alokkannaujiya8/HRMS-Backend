using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class AssetService : IAssetService
    {
        private readonly HrmsDbContext _context;

        public AssetService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<List<AssetDto>> GetAllAssetsAsync()
        {
            var assets = await _context.Assets.ToListAsync();
            var activeAssignments = await _context.AssetAssignments
                .Include(aa => aa.Employee)
                .Where(aa => aa.ReturnedDate == null)
                .ToListAsync();

            var result = new List<AssetDto>();

            foreach (var asset in assets)
            {
                var assignment = activeAssignments.FirstOrDefault(aa => aa.AssetId == asset.Id);

                result.Add(new AssetDto
                {
                    Id = asset.Id,
                    Name = asset.Name,
                    AssetTag = asset.AssetTag,
                    Category = asset.Category,
                    SerialNumber = asset.SerialNumber,
                    Status = asset.Status,
                    Description = asset.Description,
                    PurchaseDate = asset.PurchaseDate,
                    Value = asset.Value,
                    AssignedToEmployeeId = assignment?.EmployeeId,
                    AssignedToEmployeeName = assignment?.Employee?.Name,
                    AssignedDate = assignment?.AssignedDate
                });
            }

            return result;
        }

        public async Task<AssetDetailDto?> GetAssetByIdAsync(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return null;

            var activeAssignment = await _context.AssetAssignments
                .Include(aa => aa.Employee)
                .FirstOrDefaultAsync(aa => aa.AssetId == asset.Id && aa.ReturnedDate == null);

            var history = await _context.AssetAssignments
                .Include(aa => aa.Employee)
                .Where(aa => aa.AssetId == asset.Id)
                .OrderByDescending(aa => aa.AssignedDate)
                .Select(aa => new AssetAssignmentDto
                {
                    Id = aa.Id,
                    AssetId = aa.AssetId,
                    AssetName = asset.Name,
                    AssetTag = asset.AssetTag,
                    EmployeeId = aa.EmployeeId,
                    EmployeeName = aa.Employee != null ? aa.Employee.Name : string.Empty,
                    AssignedDate = aa.AssignedDate,
                    ReturnedDate = aa.ReturnedDate,
                    ConditionOnAssign = aa.ConditionOnAssign,
                    ConditionOnReturn = aa.ConditionOnReturn,
                    Notes = aa.Notes
                })
                .ToListAsync();

            return new AssetDetailDto
            {
                Id = asset.Id,
                Name = asset.Name,
                AssetTag = asset.AssetTag,
                Category = asset.Category,
                SerialNumber = asset.SerialNumber,
                Status = asset.Status,
                Description = asset.Description,
                PurchaseDate = asset.PurchaseDate,
                Value = asset.Value,
                AssignedToEmployeeId = activeAssignment?.EmployeeId,
                AssignedToEmployeeName = activeAssignment?.Employee?.Name,
                AssignedDate = activeAssignment?.AssignedDate,
                AssignmentHistory = history
            };
        }

        public async Task<AssetDto> CreateAssetAsync(AssetCreateDto dto)
        {
            if (await _context.Assets.AnyAsync(a => a.AssetTag == dto.AssetTag))
            {
                throw new InvalidOperationException($"Asset with tag '{dto.AssetTag}' already exists.");
            }

            var asset = new Asset
            {
                Name = dto.Name,
                AssetTag = dto.AssetTag,
                Category = dto.Category,
                SerialNumber = dto.SerialNumber,
                Description = dto.Description,
                PurchaseDate = dto.PurchaseDate,
                Value = dto.Value,
                Status = "Available"
            };

            await _context.Assets.AddAsync(asset);
            await _context.SaveChangesAsync();

            return new AssetDto
            {
                Id = asset.Id,
                Name = asset.Name,
                AssetTag = asset.AssetTag,
                Category = asset.Category,
                SerialNumber = asset.SerialNumber,
                Status = asset.Status,
                Description = asset.Description,
                PurchaseDate = asset.PurchaseDate,
                Value = asset.Value
            };
        }

        public async Task<AssetDto?> UpdateAssetAsync(int id, AssetUpdateDto dto)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return null;

            if (asset.AssetTag != dto.AssetTag && await _context.Assets.AnyAsync(a => a.AssetTag == dto.AssetTag && a.Id != id))
            {
                throw new InvalidOperationException($"Asset with tag '{dto.AssetTag}' already exists.");
            }

            asset.Name = dto.Name;
            asset.AssetTag = dto.AssetTag;
            asset.Category = dto.Category;
            asset.SerialNumber = dto.SerialNumber;
            asset.Description = dto.Description;
            asset.PurchaseDate = dto.PurchaseDate;
            asset.Value = dto.Value;

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                // Only allow manual status override if not assigned, or if changing to repair/retired
                if (asset.Status != "Assigned" || dto.Status == "UnderRepair" || dto.Status == "Retired")
                {
                    asset.Status = dto.Status;
                }
            }

            await _context.SaveChangesAsync();

            return new AssetDto
            {
                Id = asset.Id,
                Name = asset.Name,
                AssetTag = asset.AssetTag,
                Category = asset.Category,
                SerialNumber = asset.SerialNumber,
                Status = asset.Status,
                Description = asset.Description,
                PurchaseDate = asset.PurchaseDate,
                Value = asset.Value
            };
        }

        public async Task<bool> DeleteAssetAsync(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return false;

            if (asset.Status == "Assigned")
            {
                throw new InvalidOperationException("Cannot delete an asset that is currently assigned to an employee.");
            }

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignAssetAsync(int id, AssetAssignDto dto)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return false;

            if (asset.Status != "Available")
            {
                throw new InvalidOperationException($"Asset is not available for assignment. Current status: {asset.Status}");
            }

            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == dto.EmployeeId && e.IsActive);
            if (!employeeExists)
            {
                throw new InvalidOperationException("Active employee not found.");
            }

            var assignment = new AssetAssignment
            {
                AssetId = id,
                EmployeeId = dto.EmployeeId,
                AssignedDate = DateTime.UtcNow,
                ConditionOnAssign = dto.ConditionOnAssign,
                Notes = dto.Notes
            };

            asset.Status = "Assigned";
            await _context.AssetAssignments.AddAsync(assignment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReturnAssetAsync(int id, AssetReturnDto dto)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return false;

            if (asset.Status != "Assigned")
            {
                throw new InvalidOperationException("Asset is not currently assigned.");
            }

            var activeAssignment = await _context.AssetAssignments
                .FirstOrDefaultAsync(aa => aa.AssetId == id && aa.ReturnedDate == null);

            if (activeAssignment == null)
            {
                asset.Status = "Available"; // Correct status discrepancy if no active assignment record
                await _context.SaveChangesAsync();
                return true;
            }

            activeAssignment.ReturnedDate = DateTime.UtcNow;
            activeAssignment.ConditionOnReturn = dto.ConditionOnReturn;
            activeAssignment.Notes = string.IsNullOrWhiteSpace(dto.Notes) 
                ? activeAssignment.Notes 
                : $"{activeAssignment.Notes} | Return notes: {dto.Notes}";

            asset.Status = "Available";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AssetAssignmentDto>> GetEmployeeAssetsAsync(int employeeId)
        {
            return await _context.AssetAssignments
                .Include(aa => aa.Asset)
                .Include(aa => aa.Employee)
                .Where(aa => aa.EmployeeId == employeeId)
                .OrderByDescending(aa => aa.AssignedDate)
                .Select(aa => new AssetAssignmentDto
                {
                    Id = aa.Id,
                    AssetId = aa.AssetId,
                    AssetName = aa.Asset != null ? aa.Asset.Name : string.Empty,
                    AssetTag = aa.Asset != null ? aa.Asset.AssetTag : string.Empty,
                    EmployeeId = aa.EmployeeId,
                    EmployeeName = aa.Employee != null ? aa.Employee.Name : string.Empty,
                    AssignedDate = aa.AssignedDate,
                    ReturnedDate = aa.ReturnedDate,
                    ConditionOnAssign = aa.ConditionOnAssign,
                    ConditionOnReturn = aa.ConditionOnReturn,
                    Notes = aa.Notes
                })
                .ToListAsync();
        }
    }
}
