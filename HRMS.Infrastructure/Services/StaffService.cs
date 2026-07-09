using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class StaffService : IStaffService
    {
        private readonly HrmsDbContext _context;

        public StaffService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<StaffListResponse> GetStaffListAsync(StaffListQueryRequest request)
        {
            var query = _context.Employees.Where(e => e.IsActive).AsQueryable();
            var normalizedSearch = request.Search?.Trim();
            var normalizedDivision = request.Division?.Trim();
            var normalizedDesignation = request.Designation?.Trim();

            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                query = query.Where(e =>
                    (e.Name != null && e.Name.Contains(normalizedSearch)) ||
                    (e.Email != null && e.Email.Contains(normalizedSearch)) ||
                    (e.Mobile != null && e.Mobile.Contains(normalizedSearch)));
            }

            if (!string.IsNullOrWhiteSpace(normalizedDivision))
            {
                query = query.Where(e => e.Division == normalizedDivision);
            }

            if (!string.IsNullOrWhiteSpace(normalizedDesignation))
            {
                query = query.Where(e => e.Designation == normalizedDesignation);
            }

            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 200);
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new StaffListItemResponse
                {
                    EmployeeId = e.Id,
                    Name = e.Name,
                    Mobile = e.Mobile,
                    Email = e.Email,
                    Division = e.Division,
                    Designation = e.Designation,
                    Salary = e.Salary
                })
                .ToListAsync();

            return new StaffListResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

        public async Task<Employee?> GetProfileAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Skills)
                .Include(e => e.Remarks)
                .Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
        }

        public async Task UpdateProfileAsync(int id, EditStaffProfileRequest request)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
            if (employee == null)
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            employee.Name = request.Name;
            employee.Mobile = request.Mobile;
            employee.Email = request.Email;
            employee.Address = request.Address;
            employee.Designation = request.Designation;
            employee.Division = request.Division;
            employee.Pan = request.Pan;
            employee.Dob = request.Dob;
            employee.Gender = request.Gender;
            employee.EmploymentStatus = request.EmploymentStatus;
            employee.EmploymentType = request.EmploymentType;
            employee.IsSeventhPayCommission = request.IsSeventhPayCommission;
            employee.BankAccountNumber = request.BankAccountNumber;
            employee.IfscCode = request.IfscCode;
            employee.DepartmentId = request.DepartmentId;

            await _context.SaveChangesAsync();
        }
    }
}
