using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRMS.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedDefaultUsersAsync(HrmsDbContext context, ILogger logger)
        {
            try
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("Alok@123");
                var user = await context.Users.FirstOrDefaultAsync(u => u.Username != null && u.Username.ToLower() == "alok");

                if (user == null)
                {
                    logger.LogInformation("Seeding default Admin user 'alok'...");
                    var defaultAdmin = new AppUser
                    {
                        Username = "alok",
                        Password = passwordHash,
                        Role = "Admin",
                        Email = "alok@hrms.local",
                        IsEmailVerified = true
                    };

                    await context.Users.AddAsync(defaultAdmin);
                }
                else
                {
                    logger.LogInformation("Updating default Admin user 'alok' password...");
                    user.Password = passwordHash;
                    user.Role = "Admin";
                    user.IsEmailVerified = true;
                }

                await context.SaveChangesAsync();
                logger.LogInformation("Default Admin user 'alok' credentials synchronized successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding default user credentials.");
            }
        }
    }
}
