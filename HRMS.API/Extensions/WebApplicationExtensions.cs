using Hangfire;
using HRMS.API.Constants;
using HRMS.API.HangfireSupport;
using HRMS.API.Middleware;
using HRMS.Application.Interfaces;

namespace HRMS.API.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseHrmsRequestPipeline(this WebApplication app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors(ApiConstants.AllowAngularAppCorsPolicy);

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
