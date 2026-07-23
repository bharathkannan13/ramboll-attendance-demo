using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using RambollAttendanceAPI.Data;
using RambollAttendanceAPI.Models;

namespace RambollAttendanceAPI.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly List<string> _allowedSSIDs;

        public AttendanceRepository(IDbConnectionFactory connectionFactory, IConfiguration configuration)
        {
            _connectionFactory = connectionFactory;
            
            // Load allowed SSIDs from settings
            _allowedSSIDs = configuration.GetSection("AttendanceSettings:AllowedSSIDs")
                .Get<List<string>>() ?? new List<string>();
        }

        public async Task<Employee?> GetEmployeeBySSOAsync(string ssoIdentity)
        {
            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            await conn.OpenAsync();

            const string query = @"
                SELECT Employee_ID, SSO_Identity, FullName, Email, Department, Is_Active, Created_At 
                FROM dbo.Employee_Master 
                WHERE SSO_Identity = @SSO_Identity AND Is_Active = 1;";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@SSO_Identity", ssoIdentity);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Employee
                {
                    Employee_ID = reader.GetInt32(0),
                    SSO_Identity = reader.GetString(1),
                    FullName = reader.GetString(2),
                    Email = reader.GetString(3),
                    Department = reader.GetString(4),
                    Is_Active = reader.GetBoolean(5),
                    Created_At = reader.GetDateTime(6)
                };
            }
            return null;
        }

        public async Task LogTelemetryAsync(int employeeId, string macAddress, string hostname, string eventType, DateTime timestamp, string ssid, string ipAddress)
        {
            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            await conn.OpenAsync();
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // 1. Insert Telemetry Log
                const string insertTelemetryQuery = @"
                    INSERT INTO dbo.Telemetry_Log (Employee_ID, MAC_Address, Hostname, Event_Type, Timestamp, SSID, IP_Address)
                    VALUES (@Employee_ID, @MAC_Address, @Hostname, @Event_Type, @Timestamp, @SSID, @IP_Address);";

                using (var cmd = new SqlCommand(insertTelemetryQuery, conn, (SqlTransaction)transaction))
                {
                    cmd.Parameters.AddWithValue("@Employee_ID", employeeId);
                    cmd.Parameters.AddWithValue("@MAC_Address", macAddress);
                    cmd.Parameters.AddWithValue("@Hostname", hostname);
                    cmd.Parameters.AddWithValue("@Event_Type", eventType.ToUpper());
                    cmd.Parameters.AddWithValue("@Timestamp", timestamp);
                    cmd.Parameters.AddWithValue("@SSID", ssid);
                    cmd.Parameters.AddWithValue("@IP_Address", (object)ipAddress ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 2. Register/Update Device
                const string upsertDeviceQuery = @"
                    IF EXISTS (SELECT 1 FROM dbo.Device_Master WHERE MAC_Address = @MAC_Address)
                        UPDATE dbo.Device_Master 
                        SET Last_Seen = @Timestamp, Hostname = @Hostname 
                        WHERE MAC_Address = @MAC_Address;
                    ELSE
                        INSERT INTO dbo.Device_Master (MAC_Address, Hostname, Employee_ID, Last_Seen) 
                        VALUES (@MAC_Address, @Hostname, @Employee_ID, @Timestamp);";

                using (var cmd = new SqlCommand(upsertDeviceQuery, conn, (SqlTransaction)transaction))
                {
                    cmd.Parameters.AddWithValue("@MAC_Address", macAddress);
                    cmd.Parameters.AddWithValue("@Hostname", hostname);
                    cmd.Parameters.AddWithValue("@Employee_ID", employeeId);
                    cmd.Parameters.AddWithValue("@Timestamp", timestamp);
                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<TelemetryEvent>> GetTelemetryLogsAsync(int employeeId, DateTime date)
        {
            var logs = new List<TelemetryEvent>();
            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            await conn.OpenAsync();

            const string query = @"
                SELECT Hostname, MAC_Address, Event_Type, SSID, Timestamp 
                FROM dbo.Telemetry_Log 
                WHERE Employee_ID = @Employee_ID AND CAST(Timestamp AS DATE) = @Date
                ORDER BY Timestamp ASC;";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Employee_ID", employeeId);
            cmd.Parameters.AddWithValue("@Date", date.Date);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                logs.Add(new TelemetryEvent
                {
                    Hostname = reader.GetString(0),
                    MAC_Address = reader.GetString(1),
                    Event_Type = reader.GetString(2),
                    SSID = reader.GetString(3),
                    Timestamp = reader.GetDateTime(4)
                });
            }
            return logs;
        }

        public async Task<IEnumerable<AttendanceSummary>> GetDailySummariesAsync(DateTime date)
        {
            var summaries = new List<AttendanceSummary>();
            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            await conn.OpenAsync();

            const string query = @"
                SELECT 
                    s.Summary_ID, 
                    s.Employee_ID, 
                    e.FullName, 
                    e.Department, 
                    e.SSO_Identity, 
                    s.Work_Date, 
                    s.First_Seen, 
                    s.Last_Seen, 
                    s.Active_Hours, 
                    s.Status,
                    s.IP_Address
                FROM dbo.Attendance_Daily_Summary s
                INNER JOIN dbo.Employee_Master e ON s.Employee_ID = e.Employee_ID
                WHERE s.Work_Date = @Date
                ORDER BY s.Active_Hours DESC;";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Date", date.Date);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                summaries.Add(new AttendanceSummary
                {
                    Summary_ID = reader.GetInt32(0),
                    Employee_ID = reader.GetInt32(1),
                    FullName = reader.GetString(2),
                    Department = reader.GetString(3),
                    SSO_Identity = reader.GetString(4),
                    Work_Date = reader.GetDateTime(5),
                    First_Seen = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                    Last_Seen = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    Active_Hours = reader.GetDecimal(8),
                    Status = reader.GetString(9),
                    IP_Address = reader.IsDBNull(10) ? string.Empty : reader.GetString(10)
                });
            }
            return summaries;
        }

        public async Task RecalculateDailySummaryAsync(int employeeId, DateTime date)
        {
            var logs = (await GetTelemetryLogsAsync(employeeId, date)).ToList();

            if (!logs.Any()) return;

            // 1. First Seen & Last Seen are simply the boundaries
            DateTime firstSeen = logs.First().Timestamp;
            DateTime lastSeen = logs.Last().Timestamp;

            // 2. Active Hours algorithm using heartbeat accumulated intervals
            double totalActiveMinutes = 0;
            const double maxHeartbeatGapMinutes = 5.0; // Allowed gap threshold before counting as inactive

            for (int i = 1; i < logs.Count; i++)
            {
                var previousLog = logs[i - 1];
                var currentLog = logs[i];

                // Criteria for counting this interval as active:
                // - Previous log must NOT be a suspend event (SLEEP, LOCK, DISCONNECT)
                // - The SSID at previous log must be an authorized network SSID
                bool isPreviousStateActive = previousLog.Event_Type != "SLEEP" 
                                             && previousLog.Event_Type != "LOCK" 
                                             && previousLog.Event_Type != "DISCONNECT";
                
                bool isNetworkAuthorized = _allowedSSIDs.Any(ssid => 
                    string.Equals(ssid, previousLog.SSID, StringComparison.OrdinalIgnoreCase));

                if (isPreviousStateActive && isNetworkAuthorized)
                {
                    double gapMinutes = (currentLog.Timestamp - previousLog.Timestamp).TotalMinutes;

                    // If gap is within threshold, accumulate the time. 
                    // Otherwise, the device went offline/disconnected without sending a teardown log (so we cap the leakage).
                    if (gapMinutes <= maxHeartbeatGapMinutes)
                    {
                        totalActiveMinutes += gapMinutes;
                    }
                    else
                    {
                        // Device went offline suddenly. Accrue a default small active increment for the final heartbeat period
                        totalActiveMinutes += 0.5; // Accrue 30 seconds
                    }
                }
            }

            decimal activeHours = (decimal)Math.Round(totalActiveMinutes / 60.0, 2);

            // Determine Status:
            // - If active hours >= 8 hours -> PRESENT
            // - If active hours > 0 and < 8 -> INCOMPLETE
            // - Otherwise -> ABSENT
            string status = "ABSENT";
            if (activeHours >= 8.00m)
                status = "PRESENT";
            else if (activeHours > 0.00m)
                status = "INCOMPLETE";

            // 3. Upsert Daily Summary
            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            await conn.OpenAsync();

            const string upsertQuery = @"
                DECLARE @LatestIP VARCHAR(50);
                SELECT TOP 1 @LatestIP = IP_Address 
                FROM dbo.Telemetry_Log 
                WHERE Employee_ID = @Employee_ID AND CAST(Timestamp AS DATE) = @Work_Date 
                ORDER BY Timestamp DESC;

                MERGE INTO dbo.Attendance_Daily_Summary AS target
                USING (SELECT @Employee_ID AS Employee_ID, @Work_Date AS Work_Date) AS source
                ON target.Employee_ID = source.Employee_ID AND target.Work_Date = source.Work_Date
                WHEN MATCHED THEN
                    UPDATE SET 
                        First_Seen = @First_Seen,
                        Last_Seen = @Last_Seen,
                        Active_Hours = @Active_Hours,
                        Status = @Status,
                        IP_Address = @LatestIP,
                        Last_Updated = GETDATE()
                WHEN NOT MATCHED THEN
                    INSERT (Employee_ID, Work_Date, First_Seen, Last_Seen, Active_Hours, Status, IP_Address, Last_Updated)
                    VALUES (@Employee_ID, @Work_Date, @First_Seen, @Last_Seen, @Active_Hours, @Status, @LatestIP, GETDATE());";

            using var cmd = new SqlCommand(upsertQuery, conn);
            cmd.Parameters.AddWithValue("@Employee_ID", employeeId);
            cmd.Parameters.AddWithValue("@Work_Date", date.Date);
            cmd.Parameters.AddWithValue("@First_Seen", (object)firstSeen ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Last_Seen", (object)lastSeen ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Active_Hours", activeHours);
            cmd.Parameters.AddWithValue("@Status", status);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Employee> RegisterEmployeeAsync(string ssoIdentity, string fullName, string email, string department)
        {
            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            await conn.OpenAsync();

            const string query = @"
                INSERT INTO dbo.Employee_Master (SSO_Identity, FullName, Email, Department, Is_Active)
                OUTPUT INSERTED.Employee_ID, INSERTED.Created_At
                VALUES (@SSO_Identity, @FullName, @Email, @Department, 1);";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@SSO_Identity", ssoIdentity);
            cmd.Parameters.AddWithValue("@FullName", fullName);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Department", department);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Employee
                {
                    Employee_ID = reader.GetInt32(0),
                    SSO_Identity = ssoIdentity,
                    FullName = fullName,
                    Email = email,
                    Department = department,
                    Is_Active = true,
                    Created_At = reader.GetDateTime(1)
                };
            }
            throw new Exception("Failed to insert employee during auto-registration.");
        }

        public async Task<IEnumerable<TelemetryEventDto>> GetAllTelemetryLogsAsync(DateTime date)
        {
            var logs = new List<TelemetryEventDto>();
            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            await conn.OpenAsync();

            const string query = @"
                SELECT t.Timestamp, e.FullName, t.Hostname, t.MAC_Address, t.Event_Type, t.SSID, t.IP_Address 
                FROM dbo.Telemetry_Log t
                INNER JOIN dbo.Employee_Master e ON t.Employee_ID = e.Employee_ID
                WHERE CAST(t.Timestamp AS DATE) = @Date
                ORDER BY t.Timestamp ASC;";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Date", date.Date);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                logs.Add(new TelemetryEventDto
                {
                    Timestamp = reader.GetDateTime(0),
                    FullName = reader.GetString(1),
                    Hostname = reader.GetString(2),
                    MAC_Address = reader.GetString(3),
                    Event_Type = reader.GetString(4),
                    SSID = reader.GetString(5),
                    IP_Address = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                });
            }
            return logs;
        }

        public async Task ClearAllTelemetryAndSummariesAsync()
        {
            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            await conn.OpenAsync();

            using var transaction = conn.BeginTransaction();
            try
            {
                const string deleteTelemetry = "DELETE FROM dbo.Telemetry_Log;";
                const string deleteSummaries = "DELETE FROM dbo.Attendance_Daily_Summary;";

                using (var cmd = new SqlCommand(deleteTelemetry, conn, transaction))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var cmd = new SqlCommand(deleteSummaries, conn, transaction))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
