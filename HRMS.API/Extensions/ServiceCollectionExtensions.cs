using FluentValidation;
using HRMS.API.Authorization;
using HRMS.API.Constants;
using HRMS.API.Hubs;
using HRMS.Application.Common.Behaviors;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Interfaces;
using HRMS.Application.Services;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Repositories;
using HRMS.Infrastructure.Services;
using Hangfire;
using Hangfire.SqlServer;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

namespace HRMS.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHrmsApi(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

                    var validationProblem = new ValidationProblemDetails(errors)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation failed.",
                        Instance = context.HttpContext.Request.Path
                    };

                    return new BadRequestObjectResult(validationProblem)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

            services.AddOpenApi();

            return services;
        }

        public static IServiceCollection AddHrmsDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<HrmsDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("HRMS.Infrastructure")));

            return services;
        }

        public static IServiceCollection AddHrmsCaching(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "HRMS:";
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            services.AddScoped<ICacheService, RedisCacheService>();
            return services;
        }

        public static IServiceCollection AddHrmsHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(
                    configuration.GetConnectionString("DefaultConnection"),
                    new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.FromSeconds(15),
                        UseRecommendedIsolationLevel = true
                    }));

            services.AddHangfireServer();

            return services;
        }

        public static IServiceCollection AddHrmsServices(this IServiceCollection services)
        {
            // SignalR Service
            services.AddSignalR();
            services.AddScoped<INotificationService, NotificationService<HrmsHub>>();

            // Core Repository & Unit of Work Infrastructure
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            var applicationAssembly = typeof(HRMS.Application.Common.Results.Result).Assembly;

            // AutoMapper Registration
            services.AddAutoMapper(applicationAssembly);

            // MediatR & Pipeline Behaviors
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(applicationAssembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(applicationAssembly);

            // Domain Services
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPayslipService, PayslipService>();
            services.AddScoped<IAttendanceReportingService, AttendanceReportingService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IHrDashboardService, HrDashboardService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IHolidayService, HolidayService>();
            services.AddScoped<IMasterDataService, MasterDataService>();
            services.AddScoped<IPayrollManagementService, PayrollManagementService>();
            services.AddScoped<ISalaryManagementService, SalaryManagementService>();
            services.AddScoped<ISalaryStructureService, SalaryStructureService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<IAuditReportService, AuditReportService>();
            services.AddScoped<IReportExportService, ReportExportService>();
            services.AddScoped<ILeaveService, LeaveService>();
            services.AddScoped<IStaffService, StaffService>();
            services.AddScoped<IStaffOnboardingService, StaffOnboardingService>();
            services.AddScoped<IEmployeeDetailsService, EmployeeDetailsService>();
            services.AddScoped<IAttendanceAutomationJobService, AttendanceAutomationJobService>();
            services.AddScoped<ILeaveBalanceAutomationJobService, LeaveBalanceAutomationJobService>();
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();

            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            return services;
        }

        public static IServiceCollection AddHrmsAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(ApiConstants.BearerAuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.")))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }

        public static IServiceCollection AddHrmsCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(ApiConstants.AllowAngularAppCorsPolicy,
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
            });

            return services;
        }
    }
}
