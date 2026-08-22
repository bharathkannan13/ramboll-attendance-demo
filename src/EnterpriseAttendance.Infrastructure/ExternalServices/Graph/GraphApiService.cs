using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EnterpriseAttendance.Core.Entities;
using EnterpriseAttendance.Core.Enums;
using EnterpriseAttendance.Core.Interfaces;
using EnterpriseAttendance.Infrastructure.Data;
using Device = EnterpriseAttendance.Core.Entities.Device;
using TelemetryEvent = EnterpriseAttendance.Core.Entities.TelemetryEvent;
using ComplianceStatus = EnterpriseAttendance.Core.Enums.ComplianceStatus;

namespace EnterpriseAttendance.Infrastructure.ExternalServices.Graph
{
    public class GraphApiService : IEntraIdProvider, IIntuneProvider, IDefenderProvider
    {
        private readonly GraphServiceClient _graphClient;
        private readonly AttendanceDbContext _dbContext;
        private readonly ILogger<GraphApiService> _logger;

        public GraphApiService(
            IConfiguration configuration,
            AttendanceDbContext dbContext,
            ILogger<GraphApiService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;

            var tenantId = configuration["GraphApi:TenantId"] ?? configuration["AzureAd:TenantId"];
            var clientId = configuration["GraphApi:ClientId"] ?? configuration["AzureAd:ClientId"];
            var clientSecret = configuration["GraphApi:ClientSecret"] ?? configuration["AzureAd:ClientSecret"];

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _graphClient = new GraphServiceClient(credential);
        }

        public async Task<IReadOnlyList<Employee>> SyncEmployeesAsync()
        {
            try
            {
                _logger.LogInformation("Starting Graph API employee sync...");
                await LogApiCallAsync("GET /users", "GET", "InProgress");

                var response = await _graphClient.Users.GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Filter = "country eq 'India'";
                    requestConfiguration.QueryParameters.Select = new[] { "id", "mail", "displayName", "jobTitle", "department", "officeLocation", "employeeId", "country" };
                });

                var employees = new List<Employee>();
                var graphUsers = new List<User>();

                var pageIterator = Microsoft.Graph.PageIterator<User, UserCollectionResponse>.CreatePageIterator(_graphClient, response, (user) => {
                    graphUsers.Add(user);
                    return true;
                });

                await pageIterator.IterateAsync();

                foreach (var user in graphUsers)
                {
                    var employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.EntraObjectId == user.Id);

                    if (employee == null)
                    {
                        employee = new Employee
                        {
                            EntraObjectId = user.Id,
                            Email = user.Mail ?? $"{user.Id}@organization.com",
                            FullName = user.DisplayName ?? "Unknown Employee",
                            Title = user.JobTitle ?? "Staff",
                            EmployeeCode = user.EmployeeId ?? user.Id.Substring(0, Math.Min(8, user.Id.Length))
                        };
                        _dbContext.Employees.Add(employee);
                    }
                    else
                    {
                        employee.Email = user.Mail ?? employee.Email;
                        employee.FullName = user.DisplayName ?? employee.FullName;
                        employee.Title = user.JobTitle ?? employee.Title;
                        if (!string.IsNullOrEmpty(user.EmployeeId))
                            employee.EmployeeCode = user.EmployeeId;
                    }
                    employees.Add(employee);
                }

                await _dbContext.SaveChangesAsync();
                await LogApiCallAsync("GET /users", "GET", "Success");

                return employees;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing employees from Graph API");
                await LogApiCallAsync("GET /users", "GET", "Error", ex.Message);
                return new List<Employee>();
            }
        }

        public async Task<IReadOnlyList<Department>> SyncDepartmentsAsync()
        {
            try
            {
                var employees = await _dbContext.Employees.Include(e => e.Department).ToListAsync();
                var deptNames = employees.Where(e => e.Department != null)
                                         .Select(e => e.Department!.Name)
                                         .Distinct()
                                         .ToList();

                var departments = new List<Department>();

                foreach (var name in deptNames)
                {
                    var dept = await _dbContext.Departments.FirstOrDefaultAsync(d => d.Name == name);
                    if (dept == null)
                    {
                        dept = new Department { Name = name, Code = name.Substring(0, Math.Min(4, name.Length)).ToUpper() };
                        _dbContext.Departments.Add(dept);
                    }
                    departments.Add(dept);
                }

                await _dbContext.SaveChangesAsync();
                return departments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing departments");
                return new List<Department>();
            }
        }

        public async Task SyncManagerHierarchyAsync()
        {
            try
            {
                var employees = await _dbContext.Employees.Where(e => !string.IsNullOrEmpty(e.EntraObjectId)).ToListAsync();

                foreach (var emp in employees)
                {
                    try
                    {
                        await LogApiCallAsync($"GET /users/{emp.EntraObjectId}/manager", "GET", "InProgress");

                        var manager = await _graphClient.Users[emp.EntraObjectId].Manager.GetAsync();

                        if (manager is User managerUser)
                        {
                            var dbManager = await _dbContext.Employees.FirstOrDefaultAsync(e => e.EntraObjectId == managerUser.Id);
                            if (dbManager != null)
                            {
                                emp.ManagerId = dbManager.Id;
                            }
                        }
                        await LogApiCallAsync($"GET /users/{emp.EntraObjectId}/manager", "GET", "Success");
                    }
                    catch (Microsoft.Graph.Models.ODataErrors.ODataError ex) when (ex.ResponseStatusCode == 404)
                    {
                        await LogApiCallAsync($"GET /users/{emp.EntraObjectId}/manager", "GET", "NotFound");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to get manager for {emp.EntraObjectId}");
                        await LogApiCallAsync($"GET /users/{emp.EntraObjectId}/manager", "GET", "Error", ex.Message);
                    }
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing manager hierarchy");
            }
        }

        public async Task<IReadOnlyList<Device>> SyncManagedDevicesAsync()
        {
            try
            {
                await LogApiCallAsync("GET /deviceManagement/managedDevices", "GET", "InProgress");

                var response = await _graphClient.DeviceManagement.ManagedDevices.GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = new[] { "id", "deviceName", "serialNumber", "userId", "operatingSystem", "osVersion", "complianceState", "lastSyncDateTime" };
                });

                var devices = new List<Device>();
                var graphDevices = new List<ManagedDevice>();

                var pageIterator = Microsoft.Graph.PageIterator<ManagedDevice, ManagedDeviceCollectionResponse>.CreatePageIterator(_graphClient, response, (dev) => {
                    graphDevices.Add(dev);
                    return true;
                });

                await pageIterator.IterateAsync();

                foreach (var gDev in graphDevices)
                {
                    var device = await _dbContext.Devices.FirstOrDefaultAsync(d => d.IntuneDeviceId == gDev.Id);

                    int employeeId = 1; // Default fallback ID if no user associated
                    if (!string.IsNullOrEmpty(gDev.UserId))
                    {
                        var emp = await _dbContext.Employees.FirstOrDefaultAsync(e => e.EntraObjectId == gDev.UserId);
                        if (emp != null) employeeId = emp.Id;
                    }

                    var complianceStatus = ComplianceStatus.Unknown;
                    if (Enum.TryParse<ComplianceStatus>(gDev.ComplianceState?.ToString(), true, out var parsedStatus))
                    {
                        complianceStatus = parsedStatus;
                    }

                    if (device == null)
                    {
                        device = new Device
                        {
                            IntuneDeviceId = gDev.Id ?? Guid.NewGuid().ToString(),
                            DeviceName = gDev.DeviceName ?? "CORP-LAPTOP",
                            SerialNumber = gDev.SerialNumber ?? "SN-UNKNOWN",
                            EmployeeId = employeeId,
                            OperatingSystem = gDev.OperatingSystem ?? "Windows 11 Enterprise",
                            OSVersion = gDev.OsVersion ?? "10.0.22631",
                            ComplianceStatus = complianceStatus,
                            LastSyncTime = gDev.LastSyncDateTime?.UtcDateTime ?? DateTime.UtcNow
                        };
                        _dbContext.Devices.Add(device);
                    }
                    else
                    {
                        device.DeviceName = gDev.DeviceName ?? device.DeviceName;
                        device.SerialNumber = gDev.SerialNumber ?? device.SerialNumber;
                        device.EmployeeId = employeeId;
                        device.OperatingSystem = gDev.OperatingSystem ?? device.OperatingSystem;
                        device.OSVersion = gDev.OsVersion ?? device.OSVersion;
                        device.ComplianceStatus = complianceStatus;
                        device.LastSyncTime = gDev.LastSyncDateTime?.UtcDateTime ?? device.LastSyncTime;
                    }
                    devices.Add(device);
                }

                await _dbContext.SaveChangesAsync();
                await LogApiCallAsync("GET /deviceManagement/managedDevices", "GET", "Success");

                return devices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing managed devices from Intune");
                await LogApiCallAsync("GET /deviceManagement/managedDevices", "GET", "Error", ex.Message);
                return new List<Device>();
            }
        }

        public async Task<ComplianceStatus> GetDeviceComplianceStatusAsync(string intuneDeviceId)
        {
            try
            {
                await LogApiCallAsync($"GET /deviceManagement/managedDevices/{intuneDeviceId}", "GET", "InProgress");

                var device = await _graphClient.DeviceManagement.ManagedDevices[intuneDeviceId].GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = new[] { "complianceState" };
                });

                await LogApiCallAsync($"GET /deviceManagement/managedDevices/{intuneDeviceId}", "GET", "Success");

                if (Enum.TryParse<ComplianceStatus>(device?.ComplianceState?.ToString(), true, out var status))
                {
                    return status;
                }
                return ComplianceStatus.Unknown;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching compliance status for device {intuneDeviceId}");
                await LogApiCallAsync($"GET /deviceManagement/managedDevices/{intuneDeviceId}", "GET", "Error", ex.Message);
                return ComplianceStatus.Unknown;
            }
        }

        public Task<IReadOnlyList<TelemetryEvent>> FetchLatestTelemetryEventsAsync()
        {
            // Defender telemetry ingestion via Graph API security API (deferred in favor of Intune device sync + subnet matching)
            _logger.LogInformation("Defender telemetry fetch called — using Intune device sync + NetworkClassifier subnets.");
            return Task.FromResult<IReadOnlyList<TelemetryEvent>>(new List<TelemetryEvent>());
        }

        private async Task LogApiCallAsync(string endpoint, string method, string status, string? errorMessage = null)
        {
            try
            {
                var apiLog = new ApiLog
                {
                    Timestamp = DateTime.UtcNow,
                    Endpoint = endpoint,
                    HttpMethod = method,
                    ResponseCode = status == "Success" ? 200 : (status == "NotFound" ? 404 : 500),
                    DurationMs = 120,
                    TelemetrySource = TelemetrySource.EntraID.ToString(),
                    ErrorMessage = errorMessage ?? string.Empty
                };
                _dbContext.ApiLogs.Add(apiLog);
                await _dbContext.SaveChangesAsync();
            }
            catch
            {
                // Silently ignore logging errors
            }
        }
    }
}
