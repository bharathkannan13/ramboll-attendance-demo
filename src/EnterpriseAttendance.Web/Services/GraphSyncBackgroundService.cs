using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using EnterpriseAttendance.Core.Interfaces;

namespace EnterpriseAttendance.Web.Services
{
    public class GraphSyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GraphSyncBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public GraphSyncBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<GraphSyncBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var intervalMinutes = _configuration.GetValue<int>("TelemetrySettings:SyncIntervalMinutes", 15);
                var useMockTelemetry = _configuration.GetValue<bool>("TelemetrySettings:UseMockTelemetry", false);

                if (!useMockTelemetry)
                {
                    _logger.LogInformation("Starting Graph API sync cycle at {Time}", DateTimeOffset.Now);

                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        
                        var entraIdProvider = scope.ServiceProvider.GetRequiredService<IEntraIdProvider>();
                        var intuneProvider = scope.ServiceProvider.GetRequiredService<IIntuneProvider>();

                        await entraIdProvider.SyncEmployeesAsync();
                        await entraIdProvider.SyncDepartmentsAsync();
                        await entraIdProvider.SyncManagerHierarchyAsync();
                        await intuneProvider.SyncManagedDevicesAsync();

                        _logger.LogInformation("Completed Graph API sync cycle at {Time}", DateTimeOffset.Now);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred during the Graph API sync cycle");
                    }
                }
                else
                {
                    _logger.LogInformation("Skipping Graph API sync because UseMockTelemetry is true.");
                }

                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
        }
    }
}
