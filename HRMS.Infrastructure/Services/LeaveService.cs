using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Constants;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRMS.Infrastructure.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly HrmsDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<LeaveService> _logger;

        public LeaveService(HrmsDbContext context, IEmailService emailService, ILogger<LeaveService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
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

        public async Task<LeaveApplyResponse> ApplyLeaveAsync(int employeeId, ApplyLeaveRequest request)
        {
            if (request.FromDate.Date > request.ToDate.Date)
            {
                throw new InvalidOperationException("FromDate cannot be after ToDate.");
            }

            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == employeeId && e.IsActive);
            if (!employeeExists)
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            var leaveDays = (request.ToDate.Date - request.FromDate.Date).Days + 1;
            if (leaveDays <= 0)
            {
                throw new InvalidOperationException("Invalid leave duration.");
            }

            var leaveType = string.IsNullOrWhiteSpace(request.LeaveType) ? LeaveTypes.Casual : request.LeaveType.Trim();
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.LeaveType == leaveType);

            if (balance == null)
            {
                balance = new LeaveBalance
                {
                    EmployeeId = employeeId,
                    LeaveType = leaveType,
                    TotalLeaves = 20,
                    UsedLeaves = 0
                };
                await _context.LeaveBalances.AddAsync(balance);
            }

            if (balance.RemainingLeaves < leaveDays)
            {
                throw new InvalidOperationException($"Insufficient leave balance. Remaining: {balance.RemainingLeaves}.");
            }

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = employeeId,
                LeaveType = leaveType,
                FromDate = request.FromDate.Date,
                ToDate = request.ToDate.Date,
                Reason = request.Reason,
                Status = LeaveStatuses.Pending,
                AppliedOn = DateTime.UtcNow
            };

            await _context.LeaveRequests.AddAsync(leaveRequest);
            await _context.SaveChangesAsync();

            return new LeaveApplyResponse
            {
                Message = "Leave applied successfully.",
                LeaveRequestId = leaveRequest.Id,
                Status = leaveRequest.Status
            };
        }

        public async Task ApproveLeaveAsync(int leaveRequestId, string? comment, string actionBy)
        {
            var leaveRequest = await GetPendingLeaveRequestAsync(leaveRequestId);
            var leaveDays = (leaveRequest.ToDate.Date - leaveRequest.FromDate.Date).Days + 1;
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(x => x.EmployeeId == leaveRequest.EmployeeId && x.LeaveType == leaveRequest.LeaveType);

            if (balance == null)
            {
                throw new InvalidOperationException("Leave balance is not configured for this employee and leave type.");
            }

            if (balance.RemainingLeaves < leaveDays)
            {
                throw new InvalidOperationException($"Insufficient leave balance. Remaining: {balance.RemainingLeaves}.");
            }

            balance.UsedLeaves += leaveDays;
            leaveRequest.Status = LeaveStatuses.Approved;
            leaveRequest.ActionBy = actionBy;
            leaveRequest.ActionOn = DateTime.UtcNow;
            leaveRequest.RejectionReason = comment;

            await _context.SaveChangesAsync();
            await SendLeaveEmailAsync(leaveRequest, true, comment);
        }

        public async Task RejectLeaveAsync(int leaveRequestId, string? comment, string actionBy)
        {
            var leaveRequest = await GetPendingLeaveRequestAsync(leaveRequestId);
            leaveRequest.Status = LeaveStatuses.Rejected;
            leaveRequest.ActionBy = actionBy;
            leaveRequest.ActionOn = DateTime.UtcNow;
            leaveRequest.RejectionReason = comment;

            await _context.SaveChangesAsync();
            await SendLeaveEmailAsync(leaveRequest, false, comment);
        }

        public async Task<IReadOnlyCollection<LeaveRequest>> GetEmployeeLeavesAsync(int employeeId)
        {
            return await _context.LeaveRequests
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.AppliedOn)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<LeaveBalanceResponse>> GetEmployeeLeaveBalancesAsync(int employeeId)
        {
            return await _context.LeaveBalances
                .Where(x => x.EmployeeId == employeeId)
                .Select(x => new LeaveBalanceResponse
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    LeaveType = x.LeaveType,
                    TotalLeaves = x.TotalLeaves,
                    UsedLeaves = x.UsedLeaves,
                    RemainingLeaves = x.TotalLeaves - x.UsedLeaves
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<LeaveListItemResponse>> GetAllLeavesAsync(string? status)
        {
            var query = _context.LeaveRequests
                .Include(x => x.Employee)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == status);
            }

            return await query
                .OrderByDescending(x => x.AppliedOn)
                .Select(x => new LeaveListItemResponse
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = x.Employee != null ? x.Employee.Name : null,
                    LeaveType = x.LeaveType,
                    FromDate = x.FromDate,
                    ToDate = x.ToDate,
                    Reason = x.Reason,
                    Status = x.Status,
                    AppliedOn = x.AppliedOn,
                    ActionBy = x.ActionBy,
                    ActionOn = x.ActionOn,
                    RejectionReason = x.RejectionReason
                })
                .ToListAsync();
        }

        private async Task<LeaveRequest> GetPendingLeaveRequestAsync(int leaveRequestId)
        {
            var leaveRequest = await _context.LeaveRequests
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(x => x.Id == leaveRequestId);

            if (leaveRequest == null)
            {
                throw new KeyNotFoundException("Leave request not found.");
            }

            if (!string.Equals(leaveRequest.Status, LeaveStatuses.Pending, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only pending leave requests can be approved or rejected.");
            }

            return leaveRequest;
        }

        private async Task SendLeaveEmailAsync(LeaveRequest leaveRequest, bool approved, string? comment)
        {
            if (string.IsNullOrWhiteSpace(leaveRequest.Employee?.Email))
            {
                return;
            }

            try
            {
                var action = approved ? "Approved" : "Rejected";
                var body = approved
                    ? $@"
                        <h3>Leave Request Approved</h3>
                        <p>Hello {leaveRequest.Employee.Name},</p>
                        <p>Your leave request from <b>{leaveRequest.FromDate:dd-MMM-yyyy}</b> to <b>{leaveRequest.ToDate:dd-MMM-yyyy}</b> has been approved.</p>
                        <p>Leave Type: <b>{leaveRequest.LeaveType}</b></p>
                        <p>Regards,<br />HR Team</p>"
                    : $@"
                        <h3>Leave Request Rejected</h3>
                        <p>Hello {leaveRequest.Employee.Name},</p>
                        <p>Your leave request from <b>{leaveRequest.FromDate:dd-MMM-yyyy}</b> to <b>{leaveRequest.ToDate:dd-MMM-yyyy}</b> has been rejected.</p>
                        <p>Comments: <b>{comment ?? "N/A"}</b></p>
                        <p>Regards,<br />HR Team</p>";

                await _emailService.SendEmailAsync(
                    leaveRequest.Employee.Email,
                    $"Leave {action} Notification",
                    body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send leave notification email for LeaveRequestId {LeaveRequestId}", leaveRequest.Id);
            }
        }
    }
}
