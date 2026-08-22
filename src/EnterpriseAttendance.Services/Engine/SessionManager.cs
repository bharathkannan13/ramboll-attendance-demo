using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EnterpriseAttendance.Core.Entities;
using EnterpriseAttendance.Core.Enums;
using EnterpriseAttendance.Core.Interfaces;
using EnterpriseAttendance.Infrastructure.Data;

namespace EnterpriseAttendance.Services.Engine
{
    public class SessionManager : ISessionManager
    {
        private readonly AttendanceDbContext _context;

        public SessionManager(AttendanceDbContext context)
        {
            _context = context;
        }

        public async Task<AttendanceSession> CreateOrUpdateSessionAsync(Employee employee, Device? device, DateTime timestamp, NetworkLocationType networkType, int? officeLocationId, int? matchedNetworkId, string ip, string ssid)
        {
            var gracePeriodRule = await _context.BusinessRules.FirstOrDefaultAsync(r => r.RuleKey == "GracePeriodMinutes");
            int gracePeriodMinutes = int.TryParse(gracePeriodRule?.RuleValue, out int g) ? g : 30;

            var today = timestamp.Date;

            // Find active session for employee today on same network type
            var activeSession = await _context.AttendanceSessions
                .Where(s => s.EmployeeId == employee.Id && s.SessionDate == today && s.SessionStatus == SessionStatus.Active && s.NetworkLocationType == networkType)
                .OrderByDescending(s => s.LastSeenTime)
                .FirstOrDefaultAsync();

            if (activeSession != null)
            {
                var timeGapMinutes = (timestamp - activeSession.LastSeenTime).TotalMinutes;

                if (timeGapMinutes <= gracePeriodMinutes)
                {
                    // Continue session & update LastSeenTime
                    activeSession.LastSeenTime = timestamp;
                    activeSession.EndTime = timestamp;
                    activeSession.DurationMinutes = (timestamp - activeSession.StartTime).TotalMinutes;
                    activeSession.IPAddress = ip;
                    activeSession.DetectedSSID = ssid;

                    _context.AttendanceSessions.Update(activeSession);
                    await _context.SaveChangesAsync();
                    return activeSession;
                }
                else
                {
                    // Time gap > Grace Period → Close old session and start new one!
                    activeSession.SessionStatus = SessionStatus.Closed;
                    activeSession.ClosedReason = CloseReason.GracePeriodExpiry;
                    _context.AttendanceSessions.Update(activeSession);
                }
            }

            // Create New Attendance Session
            var newSession = new AttendanceSession
            {
                EmployeeId = employee.Id,
                DeviceId = device?.Id,
                SessionDate = today,
                StartTime = timestamp,
                LastSeenTime = timestamp,
                EndTime = timestamp,
                DurationMinutes = 0,
                NetworkLocationType = networkType,
                OfficeLocationId = officeLocationId,
                MatchedNetworkId = matchedNetworkId,
                IPAddress = ip,
                DetectedSSID = ssid,
                SessionStatus = SessionStatus.Active,
                ConfidenceScore = device?.ComplianceStatus == ComplianceStatus.Compliant ? 1.0 : 0.7
            };

            await _context.AttendanceSessions.AddAsync(newSession);
            await _context.SaveChangesAsync();
            return newSession;
        }

        public async Task CloseInactiveSessionsAsync(DateTime currentTimestamp, int gracePeriodMinutes)
        {
            var activeSessions = await _context.AttendanceSessions
                .Where(s => s.SessionStatus == SessionStatus.Active)
                .ToListAsync();

            foreach (var session in activeSessions)
            {
                var gap = (currentTimestamp - session.LastSeenTime).TotalMinutes;
                if (gap > gracePeriodMinutes)
                {
                    session.SessionStatus = SessionStatus.Closed;
                    session.ClosedReason = CloseReason.GracePeriodExpiry;
                    _context.AttendanceSessions.Update(session);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
