using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using EnterpriseAttendance.Core.Interfaces;
using EnterpriseAttendance.Infrastructure.Data;
using EnterpriseAttendance.Infrastructure.Data.SeedData;
using EnterpriseAttendance.Infrastructure.ExternalServices.Mocks;
using EnterpriseAttendance.Infrastructure.Repositories;
using EnterpriseAttendance.Services.Engine;
using EnterpriseAttendance.Services.Notifications;
using EnterpriseAttendance.Services.Services;
using EnterpriseAttendance.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// EF Core InMemory Database for zero-config out-of-the-box demonstration
builder.Services.AddDbContext<AttendanceDbContext>(options =>
    options.UseInMemoryDatabase("EnterpriseAttendanceDb"));

// Dependency Injection — Core Business Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<INetworkClassifier, NetworkClassifier>();
builder.Services.AddScoped<ISessionManager, SessionManager>();
builder.Services.AddScoped<IAttendanceEngine, AttendanceEngine>();
builder.Services.AddScoped<IOrgHierarchyService, OrgHierarchyService>();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
builder.Services.AddScoped<IReportGenerator, ExcelReportGenerator>();

// Microsoft Telemetry Providers — Conditional Registration (Mock vs Live Graph API)
var useMockTelemetry = builder.Configuration.GetValue<bool>("TelemetrySettings:UseMockTelemetry", true);

if (useMockTelemetry)
{
    // STANDALONE DEMO MODE: Use mock providers with seeded in-memory data
    builder.Services.AddScoped<IEntraIdProvider, MockEntraIdProvider>();
    builder.Services.AddScoped<IIntuneProvider, MockIntuneProvider>();
    builder.Services.AddScoped<IDefenderProvider, MockDefenderProvider>();
    builder.Services.AddScoped<ScenarioSimulator>();
}
else
{
    // LIVE PRODUCTION MODE: Use real Microsoft Graph API
    builder.Services.AddScoped<IEntraIdProvider, EnterpriseAttendance.Infrastructure.ExternalServices.Graph.GraphApiService>();
    builder.Services.AddScoped<IIntuneProvider, EnterpriseAttendance.Infrastructure.ExternalServices.Graph.GraphApiService>();
    builder.Services.AddScoped<IDefenderProvider, EnterpriseAttendance.Infrastructure.ExternalServices.Graph.GraphApiService>();

    // Register periodic Graph API sync background service
    builder.Services.AddHostedService<GraphSyncBackgroundService>();
}

// Background Services (always active)
builder.Services.AddHostedService<WeeklyManagerEmailBackgroundService>();
builder.Services.AddHostedService<EndOfDayMergeBackgroundService>();

// ===== AUTHENTICATION: Entra ID SSO (OpenID Connect) OR Demo Bypass =====
if (!useMockTelemetry)
{
    // LIVE MODE: Microsoft Entra ID Single Sign-On via OpenID Connect
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

    builder.Services.AddRazorPages()
        .AddMicrosoftIdentityUI();
}
else
{
    // DEMO MODE: Cookie-based auth with manual role switching (no Azure needed)
    builder.Services.AddAuthentication("DemoCookies")
        .AddCookie("DemoCookies", options =>
        {
            options.LoginPath = "/Auth/Login";
            options.AccessDeniedPath = "/Auth/Login";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        });
}

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("AppRole", "Administrator"));

    options.AddPolicy("AdminOrPowerUser", policy =>
        policy.RequireClaim("AppRole", "Administrator", "PowerUser"));

    options.AddPolicy("ManagerOrAbove", policy =>
        policy.RequireClaim("AppRole", "Administrator", "Manager", "PowerUser"));
});

var app = builder.Build();

// Seed Database on startup (mock mode only — live mode syncs from Graph API)
if (useMockTelemetry)
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
        await DatabaseSeeder.SeedAsync(context);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
