using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RambollAttendanceAPI.Models;

namespace RambollAttendanceAPI.Repositories
{
    public interface IAttendanceRepository
    {
        Task<Employee?> GetEmployeeBySSOAsync(string ssoIdentity);
        Task<Employee> RegisterEmployeeAsync(string ssoIdentity, string fullName, string email, string department);
        Task LogTelemetryAsync(int employeeId, string macAddress, string hostname, string eventType, DateTime timestamp, string ssid, string ipAddress);
        Task<IEnumerable<AttendanceSummary>> GetDailySummariesAsync(DateTime date);
        Task<IEnumerable<TelemetryEvent>> GetTelemetryLogsAsync(int employeeId, DateTime date);
        Task<IEnumerable<TelemetryEventDto>> GetAllTelemetryLogsAsync(DateTime date);
        Task RecalculateDailySummaryAsync(int employeeId, DateTime date);
        Task ClearAllTelemetryAndSummariesAsync();
    }
}
