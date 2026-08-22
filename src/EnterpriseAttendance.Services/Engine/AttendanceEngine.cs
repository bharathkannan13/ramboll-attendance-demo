using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EnterpriseAttendance.Core.Entities;
using EnterpriseAttendance.Core.Enums;
using EnterpriseAttendance.Core.Interfaces;
using EnterpriseAttendance.Infrastructure.Data;

namespace EnterpriseAttendance.Services.Engine
{
    public class AttendanceEngine : IAttendanceEngine
    {
        private readonly AttendanceDbContext _context;
        private readonly INetworkClassifier _networkClassifier;
        private readonly ISessionManager _sessionManager;

        public AttendanceEngine(AttendanceDbContext context, INetworkClassifier networkClassifier, ISessionManager sessionManager)
        {
            _context = context;
            _networkClassifier = networkClassifier;
            _sessionManager = sessionManager;
        }

        public async Task ProcessTelemetryEventAsync(TelemetryEvent telemetryEvent)
        {
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Id == telemetryEvent.EmployeeId);
            if (emp == null) return;

            // 1. Compliance Guard Check (Intune device compliance check)
            if (telemetryEvent.DeviceId.HasValue)
            {
                var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == telemetryEvent.DeviceId.Value);
                if (device != null && (!device.IsManaged || device.ComplianceStatus == ComplianceStatus.NonCompliant))
                {
                    // Audit alert: Non-compliant device telemetry attempt rejected!
                    var audit = new AuditLog
                    {
                        UserEmail = emp.Email,
                        Action = "Telemetry_Rejected_NonCompliant_Device",
                        EntityType = "Device",
                        EntityId = device.IntuneDeviceId,
                        Details = $"Telemetry from non-compliant/unmanaged device '{device.DeviceName}' rejected by Compliance Guard.",
                        IPAddress = telemetryEvent.IPAddress,
                        Severity = "Warning"
                    };
                    await _context.AuditLogs.AddAsync(audit);
                    telemetryEvent.ProcessedStatus = true;
                    telemetryEvent.ProcessedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return;
                }
            }

            // 2. Deduplication Filter Check
            var isDuplicate = await _context.TelemetryEvents.AnyAsync(e =>
                e.Id != telemetryEvent.Id &&
                e.EmployeeId == telemetryEvent.EmployeeId &&
                e.DeviceId == telemetryEvent.DeviceId &&
                Math.Abs((e.Timestamp - telemetryEvent.Timestamp).TotalSeconds) < 30);

            if (isDuplicate)
            {
                telemetryEvent.IsDuplicate = true;
                telemetryEvent.ProcessedStatus = true;
                telemetryEvent.ProcessedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return;
            }

            // 3. Network Classification (Ramboll Corporate Network vs Remote/VPN)
            var (locationType, officeLocationId, matchedNetworkId) = await _networkClassifier.ClassifyNetworkAsync(
                telemetryEvent.IPAddress,
                telemetryEvent.NetworkSSID,
                telemetryEvent.SubnetInfo
            );

            var dev = telemetryEvent.DeviceId.HasValue ? await _context.Devices.FindAsync(telemetryEvent.DeviceId.Value) : null;

            // 4. Update Session Manager
            await _sessionManager.CreateOrUpdateSessionAsync(
                emp,
                dev,
                telemetryEvent.Timestamp,
                locationType,
                officeLocationId,
                matchedNetworkId,
                telemetryEvent.IPAddress,
                telemetryEvent.NetworkSSID
            );

            telemetryEvent.ProcessedStatus = true;
            telemetryEvent.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task PerformEndOfDayMergeAsync(DateTime date)
        {
            var targetDate = date.Date;
            var employees = await _context.Employees.Where(e => e.IsActive).ToListAsync();

            foreach (var emp in employees)
            {
                var sessions = await _context.AttendanceSessions
                    .Where(s => s.EmployeeId == emp.Id && s.SessionDate == targetDate)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();

                if (!sessions.Any())
                {
                    // Mark Absent for working day if no sessions found
                    if (targetDate.DayOfWeek != DayOfWeek.Saturday && targetDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        var absentRecord = new DailyAttendance
                        {
                            EmployeeId = emp.Id,
                            AttendanceDate = targetDate,
                            AttendanceType = AttendanceType.Absent,
                            TotalOfficeHours = 0.0,
                            TotalSessions = 0,
                            PrimaryNetworkType = NetworkLocationType.Unknown,
                            IsHybridCompliant = false
                        };
                        await _context.DailyAttendances.AddAsync(absentRecord);
                    }
                    continue;
                }

                // Split sessions by Corporate Office vs Remote
                var officeSessions = sessions.Where(s => s.NetworkLocationType == NetworkLocationType.CorporateOffice).ToList();
                var remoteSessions = sessions.Where(s => s.NetworkLocationType != NetworkLocationType.CorporateOffice).ToList();

                DateTime? firstSeen = sessions.Min(s => s.StartTime);
                DateTime? lastSeen = sessions.Max(s => s.LastSeenTime);

                // Multi-Device & Session Hours Calculation
                double totalOfficeHours = 0.0;
                if (officeSessions.Any())
                {
                    firstSeen = officeSessions.Min(s => s.StartTime);
                    lastSeen = officeSessions.Max(s => s.LastSeenTime);
                    totalOfficeHours = CalculateNetWorkingHours(officeSessions);
                }

                bool isOfficeDay = officeSessions.Any();
                int? primaryOfficeLocId = officeSessions.FirstOrDefault()?.OfficeLocationId ?? emp.OfficeLocationId;

                var existingDaily = await _context.DailyAttendances
                    .FirstOrDefaultAsync(d => d.EmployeeId == emp.Id && d.AttendanceDate == targetDate);

                if (existingDaily != null)
                {
                    existingDaily.OfficeLocationId = primaryOfficeLocId;
                    existingDaily.AttendanceType = isOfficeDay ? AttendanceType.Office : AttendanceType.WFH;
                    existingDaily.FirstSeenTime = firstSeen;
                    existingDaily.LastSeenTime = lastSeen;
                    existingDaily.TotalOfficeHours = Math.Round(totalOfficeHours, 2);
                    existingDaily.TotalSessions = sessions.Count;
                    existingDaily.PrimaryNetworkType = isOfficeDay ? NetworkLocationType.CorporateOffice : NetworkLocationType.Remote;
                    existingDaily.IsHybridCompliant = isOfficeDay;
                    existingDaily.UpdatedAt = DateTime.UtcNow;
                    _context.DailyAttendances.Update(existingDaily);
                }
                else
                {
                    var daily = new DailyAttendance
                    {
                        EmployeeId = emp.Id,
                        AttendanceDate = targetDate,
                        OfficeLocationId = primaryOfficeLocId,
                        AttendanceType = isOfficeDay ? AttendanceType.Office : AttendanceType.WFH,
                        FirstSeenTime = firstSeen,
                        LastSeenTime = lastSeen,
                        TotalOfficeHours = Math.Round(totalOfficeHours, 2),
                        TotalSessions = sessions.Count,
                        PrimaryNetworkType = isOfficeDay ? NetworkLocationType.CorporateOffice : NetworkLocationType.Remote,
                        IsHybridCompliant = isOfficeDay
                    };
                    await _context.DailyAttendances.AddAsync(daily);
                }
            }

            await _context.SaveChangesAsync();
        }

        private double CalculateNetWorkingHours(List<AttendanceSession> sessions)
        {
            if (!sessions.Any()) return 0.0;

            // Merge overlapping intervals across multiple devices into unified continuous timeline
            var intervals = sessions
                .Select(s => (Start: s.StartTime, End: s.LastSeenTime))
                .OrderBy(i => i.Start)
                .ToList();

            var mergedIntervals = new List<(DateTime Start, DateTime End)>();
            var current = intervals[0];

            for (int i = 1; i < intervals.Count; i++)
            {
                if (intervals[i].Start <= current.End)
                {
                    // Overlapping interval -> extend end time
                    current.End = intervals[i].End > current.End ? intervals[i].End : current.End;
                }
                else
                {
                    mergedIntervals.Add(current);
                    current = intervals[i];
                }
            }
            mergedIntervals.Add(current);

            // Sum up net active minutes
            double totalMinutes = mergedIntervals.Sum(i => (i.End - i.Start).TotalMinutes);
            return totalMinutes / 60.0;
        }

        public async Task AggregateWeeklySummariesAsync(DateTime weekStartDate)
        {
            var weekEnd = weekStartDate.AddDays(6);
            var employees = await _context.Employees.Include(e => e.Department).Where(e => e.IsActive).ToListAsync();

            foreach (var emp in employees)
            {
                var dailyRecords = await _context.DailyAttendances
                    .Where(d => d.EmployeeId == emp.Id && d.AttendanceDate >= weekStartDate && d.AttendanceDate <= weekEnd)
                    .ToListAsync();

                int officeDays = dailyRecords.Count(d => d.AttendanceType == AttendanceType.Office);
                int wfhDays = dailyRecords.Count(d => d.AttendanceType == AttendanceType.WFH);
                int absentDays = dailyRecords.Count(d => d.AttendanceType == AttendanceType.Absent);
                double totalOfficeHours = dailyRecords.Sum(d => d.TotalOfficeHours);

                int targetDays = 3;
                PolicyComplianceStatus complianceStatus;
                if (officeDays >= targetDays) complianceStatus = PolicyComplianceStatus.Met;
                else if (officeDays == targetDays - 1) complianceStatus = PolicyComplianceStatus.PartiallyMet;
                else complianceStatus = PolicyComplianceStatus.NonCompliant;

                var summary = await _context.AttendanceSummaries.FirstOrDefaultAsync(s =>
                    s.EmployeeId == emp.Id && s.PeriodType == PeriodType.Weekly && s.PeriodStartDate == weekStartDate);

                if (summary == null)
                {
                    summary = new AttendanceSummary
                    {
                        EmployeeId = emp.Id,
                        DepartmentId = emp.DepartmentId,
                        PeriodType = PeriodType.Weekly,
                        PeriodStartDate = weekStartDate,
                        PeriodEndDate = weekEnd
                    };
                    await _context.AttendanceSummaries.AddAsync(summary);
                }

                summary.TotalWorkingDays = 5;
                summary.OfficeDaysCount = officeDays;
                summary.WFHDaysCount = wfhDays;
                summary.AbsentDaysCount = absentDays;
                summary.TotalOfficeHours = Math.Round(totalOfficeHours, 2);
                summary.AverageOfficeHoursPerDay = officeDays > 0 ? Math.Round(totalOfficeHours / officeDays, 2) : 0;
                summary.HybridCompliancePercentage = Math.Round((double)officeDays / targetDays * 100, 1);
                summary.PolicyTargetOfficeDays = targetDays;
                summary.PolicyComplianceStatus = complianceStatus;
            }

            await _context.SaveChangesAsync();
        }

        public async Task AggregateMonthlySummariesAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var employees = await _context.Employees.Include(e => e.Department).Where(e => e.IsActive).ToListAsync();

            foreach (var emp in employees)
            {
                var dailyRecords = await _context.DailyAttendances
                    .Where(d => d.EmployeeId == emp.Id && d.AttendanceDate >= startDate && d.AttendanceDate <= endDate)
                    .ToListAsync();

                int officeDays = dailyRecords.Count(d => d.AttendanceType == AttendanceType.Office);
                int wfhDays = dailyRecords.Count(d => d.AttendanceType == AttendanceType.WFH);
                int absentDays = dailyRecords.Count(d => d.AttendanceType == AttendanceType.Absent);
                double totalOfficeHours = dailyRecords.Sum(d => d.TotalOfficeHours);

                var summary = await _context.AttendanceSummaries.FirstOrDefaultAsync(s =>
                    s.EmployeeId == emp.Id && s.PeriodType == PeriodType.Monthly && s.PeriodStartDate == startDate);

                if (summary == null)
                {
                    summary = new AttendanceSummary
                    {
                        EmployeeId = emp.Id,
                        DepartmentId = emp.DepartmentId,
                        PeriodType = PeriodType.Monthly,
                        PeriodStartDate = startDate,
                        PeriodEndDate = endDate
                    };
                    await _context.AttendanceSummaries.AddAsync(summary);
                }

                summary.TotalWorkingDays = dailyRecords.Count;
                summary.OfficeDaysCount = officeDays;
                summary.WFHDaysCount = wfhDays;
                summary.AbsentDaysCount = absentDays;
                summary.TotalOfficeHours = Math.Round(totalOfficeHours, 2);
                summary.AverageOfficeHoursPerDay = officeDays > 0 ? Math.Round(totalOfficeHours / officeDays, 2) : 0;
                summary.HybridCompliancePercentage = Math.Round((double)officeDays / 12 * 100, 1); // 12 days target per month
                summary.PolicyTargetOfficeDays = 12;
                summary.PolicyComplianceStatus = officeDays >= 12 ? PolicyComplianceStatus.Met : PolicyComplianceStatus.NonCompliant;
            }

            await _context.SaveChangesAsync();
        }
    }
}
