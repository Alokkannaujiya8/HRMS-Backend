using Hangfire;
using Hangfire.SqlServer;
using HRMS.API.Authorization;
using HRMS.API.HangfireSupport;
using HRMS.Application.Interfaces;
using HRMS.Application.Services;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Repositories;
using HRMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddOpenApi();

builder.Services.AddDbContext<HrmsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("HRMS.Infrastructure")
    ));

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.FromSeconds(15),
            UseRecommendedIsolationLevel = true
        }));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPayslipService, PayslipService>();
builder.Services.AddScoped<IAttendanceAutomationJobService, AttendanceAutomationJobService>();
builder.Services.AddScoped<ILeaveBalanceAutomationJobService, LeaveBalanceAutomationJobService>();

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddAuthentication("Bearer")
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.")))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowAngularApp");

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardBasicAuthFilter(builder.Configuration) }
});

app.UseAuthentication();
app.UseAuthorization();

var hangfireTimeZoneId = builder.Configuration["Hangfire:TimeZoneId"] ?? "India Standard Time";
var hangfireTimeZone = TimeZoneInfo.FindSystemTimeZoneById(hangfireTimeZoneId);

RecurringJob.AddOrUpdate<IAttendanceAutomationJobService>(
    "mark-absent-nightly",
    job => job.MarkAbsentEmployeesAsync(),
    "59 23 * * *",
    new RecurringJobOptions { TimeZone = hangfireTimeZone });

RecurringJob.AddOrUpdate<ILeaveBalanceAutomationJobService>(
    "recalculate-monthly-leave-balances",
    job => job.RecalculateMonthlyLeaveBalancesAsync(),
    Cron.Monthly(1, 0, 5),
    new RecurringJobOptions { TimeZone = hangfireTimeZone });

app.MapControllers();

app.Run();