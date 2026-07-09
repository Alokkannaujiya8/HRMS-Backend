using HRMS.Application.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly HrmsDbContext _context;

        public UserManagementService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserSummaryResponse>> GetUsersAsync()
        {
            return await _context.Users
                .Select(x => new UserSummaryResponse
                {
                    Id = x.Id,
                    Username = x.Username,
                    Role = x.Role,
                    EmployeeId = x.EmployeeId
                })
                .ToListAsync();
        }
    }
}
