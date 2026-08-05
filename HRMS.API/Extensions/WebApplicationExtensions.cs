using Hangfire;
using HRMS.API.Constants;
using HRMS.API.HangfireSupport;
using HRMS.API.Hubs;
using HRMS.API.Middleware;
using HRMS.Application.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HRMS.API.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseHrmsRequestPipeline(this WebApplication app)
        {
            // Allow CORS before HTTPS redirection to prevent CORS preflight redirect blocks
            app.UseCors(ApiConstants.AllowAngularAppCorsPolicy);

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<PerformanceLoggingMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();

            app.UseHangfireDashboard(
                "/hangfire",
                new DashboardOptions
                {
                    Authorization = new[]
                    {
                        new HangfireDashboardBasicAuthFilter(app.Configuration),
                    },
                }
            );

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<HrmsHub>("/hubs/notifications");

            return app;
        }

        public static async Task<WebApplication> ApplyPendingMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HrmsDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebApplication>>();

            logger.LogInformation("Applying pending EF Core database migrations...");
            await dbContext.Database.MigrateAsync();

            logger.LogInformation("Seeding default database credentials...");
            await DbInitializer.SeedDefaultUsersAsync(dbContext, logger);

            return app;
        }

        public static WebApplication MapHrmsRecurringJobs(this WebApplication app)
        {
            var hangfireTimeZoneId =
                app.Configuration["Hangfire:TimeZoneId"] ?? ApiConstants.DefaultHangfireTimeZoneId;
            var hangfireTimeZone = TimeZoneInfo.FindSystemTimeZoneById(hangfireTimeZoneId);

            RecurringJob.AddOrUpdate<IAttendanceAutomationJobService>(
                HangfireJobIds.MarkAbsentNightly,
                job => job.MarkAbsentEmployeesAsync(),
                "59 23 * * *",
                new RecurringJobOptions { TimeZone = hangfireTimeZone }
            );

            RecurringJob.AddOrUpdate<ILeaveBalanceAutomationJobService>(
                HangfireJobIds.RecalculateMonthlyLeaveBalances,
                job => job.RecalculateMonthlyLeaveBalancesAsync(),
                Cron.Monthly(1, 0, 5),
                new RecurringJobOptions { TimeZone = hangfireTimeZone }
            );

            return app;
        }
    }
}
