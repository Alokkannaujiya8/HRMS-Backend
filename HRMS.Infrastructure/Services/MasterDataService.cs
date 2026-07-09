using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly HrmsDbContext _context;

        public MasterDataService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<MasterDataResponse> GetStaffMastersAsync()
        {
            var divisions = await _context.Employees
                .Where(e => e.IsActive && e.Division != null && e.Division != "")
                .Select(e => e.Division!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var designations = await _context.Employees
                .Where(e => e.IsActive && e.Designation != null && e.Designation != "")
                .Select(e => e.Designation!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var departments = await _context.Departments
                .AsNoTracking()
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentMasterItem
                {
                    Id = d.Id,
                    Name = d.Name ?? string.Empty,
                    Code = d.Code ?? string.Empty
                })
                .ToListAsync();

            return new MasterDataResponse
            {
                Divisions = divisions,
                Designations = designations,
                Departments = departments
            };
        }
    }
}
