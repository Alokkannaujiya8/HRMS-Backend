using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Constants;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly HrmsDbContext _context;

        public AttendanceService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<int?> ResolveEmployeeIdAsync(string? employeeIdClaim, string? username)
        {
            if (int.TryParse(employeeIdClaim, out var employeeId))
            {
                return employeeId;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            return await _context.Employees
                .Where(x => x.IsActive && x.Email == username)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<AttendanceCheckInResponse> CheckInAsync(int employeeId)
        {
            var today = DateTime.UtcNow.Date;
            var existing = await _context.Attendances
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.AttendanceDate == today);

            if (existing != null && existing.CheckInTime.HasValue)
            {
                throw new InvalidOperationException("Already checked in for today.");
            }

            if (existing == null)
            {
                existing = new Attendance
                {
                    EmployeeId = employeeId,
                    AttendanceDate = today,
                    CheckInTime = DateTime.UtcNow,
                    Status = AttendanceStatuses.Present
                };

                await _context.Attendances.AddAsync(existing);
            }
            else
            {
                existing.CheckInTime = DateTime.UtcNow;
                existing.Status = AttendanceStatuses.Present;
            }

            await _context.SaveChangesAsync();

            return new AttendanceCheckInResponse
            {
                Message = "Check-in successful.",
                Id = existing.Id,
                CheckInTime = existing.CheckInTime
            };
        }

        public async Task<AttendanceCheckOutResponse> CheckOutAsync(int employeeId)
        {
            var today = DateTime.UtcNow.Date;
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.AttendanceDate == today);

            if (attendance == null || !attendance.CheckInTime.HasValue)
            {
                throw new InvalidOperationException("Check-in not found for today.");
            }

            if (attendance.CheckOutTime.HasValue)
            {
                throw new InvalidOperationException("Already checked out for today.");
            }

            attendance.CheckOutTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var totalHours = (attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours;
            return new AttendanceCheckOutResponse
            {
                Message = "Check-out successful.",
                Id = attendance.Id,
                CheckOutTime = attendance.CheckOutTime,
                TotalHours = Math.Round(totalHours, 2)
            };
        }

        public Task<IReadOnlyCollection<AttendanceListItemResponse>> GetEmployeeAttendanceAsync(int employeeId, DateTime? fromDate, DateTime? toDate)
        {
            return GetAttendanceAsync(employeeId, fromDate, toDate);
        }

        public Task<IReadOnlyCollection<AttendanceListItemResponse>> GetAllAttendanceAsync(int? employeeId, DateTime? fromDate, DateTime? toDate)
        {
            return GetAttendanceAsync(employeeId, fromDate, toDate);
        }

        private async Task<IReadOnlyCollection<AttendanceListItemResponse>> GetAttendanceAsync(int? employeeId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Attendances
                .Include(x => x.Employee)
                .AsQueryable();

            if (employeeId.HasValue)
            {
                query = query.Where(x => x.EmployeeId == employeeId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.AttendanceDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.AttendanceDate <= toDate.Value.Date);
            }

            return await query
                .OrderByDescending(x => x.AttendanceDate)
                .Select(x => new AttendanceListItemResponse
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = x.Employee != null ? x.Employee.Name : null,
                    AttendanceDate = x.AttendanceDate,
                    CheckInTime = x.CheckInTime,
                    CheckOutTime = x.CheckOutTime,
                    Status = x.Status
                })
                .ToListAsync();
        }
    }
}
