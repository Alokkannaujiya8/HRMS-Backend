

namespace HRMS.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(string username, string role, int? employeeId = null, IReadOnlyCollection<string>? permissions = null);
    }
}
