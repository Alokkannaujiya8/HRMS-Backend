using HRMS.Application.Interfaces;
using HRMS.Domain.Constants;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class AttendanceAutomationJobService : IAttendanceAutomationJobService
    {
        private readonly HrmsDbContext _context;

        public AttendanceAutomationJobService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task MarkAbsentEmployeesAsync()
        {
            var today = DateTime.UtcNow.Date;

            var activeEmployees = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => e.Id)
                .ToListAsync();

            if (activeEmployees.Count == 0)
            {
                return;
            }

            var attendanceMap = await _context.Attendances
                .Where(a => a.AttendanceDate == today)
                .ToDictionaryAsync(a => a.EmployeeId);

            foreach (var employeeId in activeEmployees)
            {
                if (!attendanceMap.TryGetValue(employeeId, out var attendance))
                {
                    await _context.Attendances.AddAsync(new Attendance
                    {
                        EmployeeId = employeeId,
                        AttendanceDate = today,
                        Status = AttendanceStatuses.Absent
                    });

                    continue;
                }

                if (!attendance.CheckInTime.HasValue)
                {
                    attendance.Status = AttendanceStatuses.Absent;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
