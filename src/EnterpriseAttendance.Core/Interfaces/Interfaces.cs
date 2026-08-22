using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EnterpriseAttendance.Core.Entities;
using EnterpriseAttendance.Core.Enums;
using EnterpriseAttendance.Core.Models;

namespace EnterpriseAttendance.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }

    public interface IUnitOfWork : IDisposable
    {
        IRepository<Employee> Employees { get; }
        IRepository<Department> Departments { get; }
        IRepository<OfficeLocation> OfficeLocations { get; }
        IRepository<OfficeNetwork> OfficeNetworks { get; }
        IRepository<Device> Devices { get; }
        IRepository<AttendanceSession> AttendanceSessions { get; }
        IRepository<DailyAttendance> DailyAttendances { get; }
        IRepository<AttendanceSummary> AttendanceSummaries { get; }
        IRepository<TelemetryEvent> TelemetryEvents { get; }
        IRepository<BusinessRule> BusinessRules { get; }
        IRepository<AuditLog> AuditLogs { get; }
        IRepository<ApiLog> ApiLogs { get; }
        IRepository<EmailNotificationLog> EmailNotificationLogs { get; }
        IRepository<EmailTemplate> EmailTemplates { get; }
        IRepository<WeeklyReportLog> WeeklyReportLogs { get; }
        IRepository<SystemConfiguration> SystemConfigurations { get; }

        Task<int> CompleteAsync();
    }

    public interface IAttendanceEngine
    {
        Task ProcessTelemetryEventAsync(TelemetryEvent telemetryEvent);
        Task PerformEndOfDayMergeAsync(DateTime date);
        Task AggregateWeeklySummariesAsync(DateTime weekStartDate);
        Task AggregateMonthlySummariesAsync(int year, int month);
    }

    public interface INetworkClassifier
    {
        Task<(NetworkLocationType LocationType, int? OfficeLocationId, int? MatchedNetworkId)> ClassifyNetworkAsync(string ipAddress, string ssid, string subnet);
    }

    public interface ISessionManager
    {
        Task<AttendanceSession> CreateOrUpdateSessionAsync(Employee employee, Device? device, DateTime timestamp, NetworkLocationType networkType, int? officeLocationId, int? matchedNetworkId, string ip, string ssid);
        Task CloseInactiveSessionsAsync(DateTime currentTimestamp, int gracePeriodMinutes);
    }

    public interface IEntraIdProvider
    {
        Task<IReadOnlyList<Employee>> SyncEmployeesAsync();
        Task<IReadOnlyList<Department>> SyncDepartmentsAsync();
        Task SyncManagerHierarchyAsync();
    }

    public interface IIntuneProvider
    {
        Task<IReadOnlyList<Device>> SyncManagedDevicesAsync();
        Task<ComplianceStatus> GetDeviceComplianceStatusAsync(string intuneDeviceId);
    }

    public interface IDefenderProvider
    {
        Task<IReadOnlyList<TelemetryEvent>> FetchLatestTelemetryEventsAsync();
    }

    public interface IOrgHierarchyService
    {
        Task<IReadOnlyList<Employee>> GetReportingSubtreeAsync(int managerEmployeeId);
        Task<bool> IsEmployeeInManagerSubtreeAsync(int managerEmployeeId, int targetEmployeeId);
        Task<OrgNodeDto?> GetOrgChartTreeAsync(int rootManagerId);
    }

    public interface IEmailNotificationService
    {
        Task SendWeeklyManagerReportAsync(int managerId, DateTime weekStartDate);
        Task SendMonthlyManagerSummaryAsync(int managerId, int year, int month);
        Task<IReadOnlyList<EmailNotificationLog>> GetEmailLogsAsync();
    }

    public interface IReportGenerator
    {
        Task<byte[]> GenerateWeeklyManagerExcelReportAsync(int managerId, DateTime weekStartDate);
        Task<byte[]> GenerateMonthlyManagerExcelReportAsync(int managerId, int year, int month);
    }
}
