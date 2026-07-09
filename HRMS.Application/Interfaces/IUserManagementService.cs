namespace HRMS.Application.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<UserSummaryResponse>> GetUsersAsync();
    }

    public class UserSummaryResponse
    {
        public int Id { get; set; }

        public string? Username { get; set; }

        public string? Role { get; set; }

        public int? EmployeeId { get; set; }
    }
}
