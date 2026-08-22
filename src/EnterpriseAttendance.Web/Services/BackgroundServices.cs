using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EnterpriseAttendance.Core.Interfaces;

namespace EnterpriseAttendance.Web.Services
{
    public class WeeklyManagerEmailBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WeeklyManagerEmailBackgroundService> _logger;

        public WeeklyManagerEmailBackgroundService(IServiceProvider serviceProvider, ILogger<WeeklyManagerEmailBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Weekly Manager Email Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    // Check if Monday 9:00 AM
                    if (now.DayOfWeek == DayOfWeek.Monday && now.Hour == 9 && now.Minute == 0)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var managers = await unitOfWork.Employees.FindAsync(e => e.Role == Core.Enums.UserRole.Manager && e.IsActive);
                        var weekStart = DateTime.Today.AddDays(-7); // Preceding week

                        foreach (var mgr in managers)
                        {
                            _logger.LogInformation("Generating Weekly Attendance Email for Manager: {Email}", mgr.Email);
                            await emailService.SendWeeklyManagerReportAsync(mgr.Id, weekStart);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing Weekly Manager Email Background Service.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    public class EndOfDayMergeBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EndOfDayMergeBackgroundService> _logger;

        public EndOfDayMergeBackgroundService(IServiceProvider serviceProvider, ILogger<EndOfDayMergeBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    if (now.Hour == 23 && now.Minute == 55)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var engine = scope.ServiceProvider.GetRequiredService<IAttendanceEngine>();
                        _logger.LogInformation("Executing End-of-Day Session Merge for {Date}", DateTime.Today);
                        await engine.PerformEndOfDayMergeAsync(DateTime.Today);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing End-of-Day Merge Background Service.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
