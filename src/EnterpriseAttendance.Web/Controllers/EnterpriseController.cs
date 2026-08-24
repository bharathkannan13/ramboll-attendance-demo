using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterpriseAttendance.Infrastructure.Data;

namespace EnterpriseAttendance.Web.Controllers
{
    // =====================================================================
    // ENTERPRISE CONTROLLER — 18-Table Specification API Endpoints
    // =====================================================================
    [ApiController]
    [Route("api/[controller]")]
    public class EnterpriseController : ControllerBase
    {
        private readonly AttendanceDbContext _context;

        public EnterpriseController(AttendanceDbContext context)
        {
            _context = context;
        }

        // 1. Governance & Ownership
        [HttpGet("governance")]
        public async Task<IActionResult> GetGovernance()
        {
            var ownership = await _context.ApplicationOwnerships.FirstOrDefaultAsync();
            return Ok(ownership);
        }

        // 2. RBAC Roles & Permissions
        [HttpGet("rbac")]
        public async Task<IActionResult> GetRbac()
        {
            var roles = await _context.RoleMasters.ToListAsync();
            return Ok(roles);
        }

        // 3. Multi-Location Offices (Chennai, Bangalore, Mumbai, Pune, Delhi, Noida, Hyderabad, Gurugram)
        [HttpGet("offices")]
        public async Task<IActionResult> GetOffices()
        {
            var offices = await _context.OfficeMasters.ToListAsync();
            return Ok(offices);
        }

        // 4. Work Modes (Office, WFH, Client Site, Travel)
        [HttpGet("work-modes")]
        public async Task<IActionResult> GetWorkModes()
        {
            var modes = await _context.WorkModeMasters.ToListAsync();
            return Ok(modes);
        }

        // 5. Security Audit Logs
        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs()
        {
            var logs = await _context.SecurityAuditLogs.OrderByDescending(a => a.Action_Time).Take(50).ToListAsync();
            return Ok(logs);
        }

        // 6. Active Login Sessions
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var sessions = await _context.LoginSessionLogs.ToListAsync();
            return Ok(sessions);
        }

        // 7. System Error Logs
        [HttpGet("error-logs")]
        public async Task<IActionResult> GetErrorLogs()
        {
            var errors = await _context.ErrorLogs.OrderByDescending(e => e.Created_Time).Take(50).ToListAsync();
            return Ok(errors);
        }

        // 8. Data Retention Policies
        [HttpGet("retention")]
        public async Task<IActionResult> GetRetention()
        {
            var retention = await _context.RetentionConfigs.ToListAsync();
            return Ok(retention);
        }

        // 9. Backup & Recovery Logs
        [HttpGet("backups")]
        public async Task<IActionResult> GetBackups()
        {
            var backups = await _context.BackupLogs.ToListAsync();
            return Ok(backups);
        }

        // 10. Integration Layer Config (Entra ID, Workday, Cisco ISE, Intune, Active Directory)
        [HttpGet("integrations")]
        public async Task<IActionResult> GetIntegrations()
        {
            var integrations = await _context.IntegrationConfigs.ToListAsync();
            return Ok(integrations);
        }

        // 11. Device Intelligence Inventory
        [HttpGet("devices")]
        public async Task<IActionResult> GetDevices()
        {
            var devices = await _context.Devices.Take(50).ToListAsync();
            return Ok(devices);
        }

        // 12. Cybersecurity Threat & Risk Logs
        [HttpGet("cybersecurity-risks")]
        public async Task<IActionResult> GetCybersecurityRisks()
        {
            var risks = await _context.AttendanceRiskLogs.ToListAsync();
            return Ok(risks);
        }

        // 13. AI & Predictive Analytics
        [HttpGet("ai-analytics")]
        public async Task<IActionResult> GetAiAnalytics()
        {
            var analytics = await _context.AnalyticsLogs.ToListAsync();
            return Ok(analytics);
        }
    }
}
