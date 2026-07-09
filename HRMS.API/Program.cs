using HRMS.API.Extensions;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder
    .Services.AddHrmsApi()
    .AddHrmsDatabase(builder.Configuration)
    .AddHrmsHangfire(builder.Configuration)
    .AddAuthorization()
    .AddHrmsServices()
    .AddHrmsAuthentication(builder.Configuration)
    .AddHrmsCors();

var app = builder.Build();

app.UseHrmsRequestPipeline();
app.MapHrmsRecurringJobs();

app.Run();
