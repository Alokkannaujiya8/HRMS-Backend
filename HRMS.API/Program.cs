using HRMS.API.Extensions;
using QuestPDF.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder
    .Services.AddHrmsApi()
    .AddHrmsDatabase(builder.Configuration)
    .AddHrmsCaching(builder.Configuration)
    .AddHrmsHangfire(builder.Configuration)
    .AddAuthorization()
    .AddHrmsServices()
    .AddHrmsAuthentication(builder.Configuration)
    .AddHrmsCors();

var app = builder.Build();

await app.ApplyPendingMigrationsAsync();

app.UseHrmsRequestPipeline();
app.MapHrmsRecurringJobs();

app.Run();
