using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterpriseAttendance.Core.Entities;
using EnterpriseAttendance.Core.Enums;
using EnterpriseAttendance.Core.Interfaces;
using EnterpriseAttendance.Core.Models;
using EnterpriseAttendance.Infrastructure.Data;

namespace EnterpriseAttendance.Web.Controllers
{
    // =====================================================================
    // AUTH CONTROLLER — SSO Login/Logout + Demo Cookie Login
    // =====================================================================
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AttendanceDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AttendanceDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        /// <summary>
        /// Demo Mode: Password-based authentication (Username: Ramboll / Password: Ramboll12345)
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var identifier = string.IsNullOrWhiteSpace(request.Email) ? request.Username : request.Email;
            identifier = identifier?.Trim() ?? string.Empty;

            Employee? user = null;

            // Demo Admin Credential Shortcut: Username "Ramboll" or "rajesh.sharma@bkrangroup.com"
            if (identifier.Equals("Ramboll", StringComparison.OrdinalIgnoreCase) ||
                identifier.Equals("ramboll@bkrangroup.com", StringComparison.OrdinalIgnoreCase) ||
                identifier.Equals("rajesh.sharma@bkrangroup.com", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(request.Password) && request.Password != "Ramboll12345")
                {
                    return Unauthorized(new { message = "Incorrect password. Default demo password is 'Ramboll12345'." });
                }

                user = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.OfficeLocation)
                    .FirstOrDefaultAsync(e => e.Role == UserRole.Administrator)
                    ?? await _context.Employees.Include(e => e.Department).Include(e => e.OfficeLocation).FirstOrDefaultAsync();
            }
            else
            {
                user = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.OfficeLocation)
                    .FirstOrDefaultAsync(e => e.Email.Equals(identifier, StringComparison.OrdinalIgnoreCase) ||
                                              e.FullName.Contains(identifier, StringComparison.OrdinalIgnoreCase));

                if (user != null && !string.IsNullOrWhiteSpace(request.Password) && request.Password != "Ramboll12345")
                {
                    return Unauthorized(new { message = "Incorrect password. Default demo password is 'Ramboll12345'." });
                }
            }

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "Invalid credentials. Use Username: 'Ramboll' and Password: 'Ramboll12345'." });
            }

            // Create cookie claims for demo session
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("AppRole", user.Role.ToString()),
                new Claim("EmployeeId", user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, "DemoCookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("DemoCookies", principal);

            return Ok(new
            {
                success = true,
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Title,
                    Role = user.Role.ToString(),
                    Department = user.Department?.Name,
                    Office = user.OfficeLocation?.Name
                },
                redirectUrl = (user.Role == UserRole.Administrator || user.Role == UserRole.PowerUser)
                    ? "/Admin"
                    : "/Manager"
            });
        }

        /// <summary>
        /// Returns the currently logged-in user's profile from claims
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null) return Unauthorized(new { message = "User not authenticated" });

            var user = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.OfficeLocation)
                .FirstOrDefaultAsync(e => e.Id == employeeId.Value);

            if (user == null) return NotFound(new { message = "User not found" });

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Title,
                Role = user.Role.ToString(),
                Department = user.Department?.Name,
                Office = user.OfficeLocation?.Name
            });
        }

        /// <summary>
        /// Logout (works for both SSO and Demo Cookie)
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Ok(new { message = "Signed out successfully", redirectUrl = "/Auth/Login" });
        }

        private int? GetCurrentEmployeeId()
        {
            var idClaim = User.FindFirst("EmployeeId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            return idClaim != null && int.TryParse(idClaim.Value, out var id) ? id : null;
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // =====================================================================
    // ATTENDANCE CONTROLLER — User-level attendance details
    // =====================================================================
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly AttendanceDbContext _context;

        public AttendanceController(AttendanceDbContext context)
        {
            _context = context;
        }

        [HttpGet("user-details/{employeeId}")]
        public async Task<IActionResult> GetUserDetails(int employeeId)
        {
            var emp = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.OfficeLocation)
                .Include(e => e.Manager)
                .Include(e => e.Devices)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (emp == null) return NotFound(new { message = "Employee not found." });

            var recentDaily = await _context.DailyAttendances
                .Include(d => d.OfficeLocation)
                .Where(d => d.EmployeeId == employeeId)
                .OrderByDescending(d => d.AttendanceDate)
                .Take(14)
                .ToListAsync();

            int officeDays = recentDaily.Count(d => d.AttendanceType == AttendanceType.Office);
            int wfhDays = recentDaily.Count(d => d.AttendanceType == AttendanceType.WFH);
            double totalHours = recentDaily.Sum(d => d.TotalOfficeHours);

            var activeDevice = emp.Devices.FirstOrDefault(d => d.IsManaged && d.ComplianceStatus == ComplianceStatus.Compliant);

            return Ok(new
            {
                Employee = new
                {
                    emp.Id,
                    emp.FullName,
                    emp.Email,
                    emp.Title,
                    emp.EmployeeCode,
                    Department = emp.Department?.Name,
                    OfficeLocation = emp.OfficeLocation?.Name,
                    ManagerName = emp.Manager?.FullName ?? "Executive Leadership",
                    DeviceName = activeDevice?.DeviceName ?? "BKRAN-LAPTOP-CORP",
                    ComplianceStatus = activeDevice?.ComplianceStatus.ToString() ?? "Compliant"
                },
                Metrics = new
                {
                    OfficeDaysCount = officeDays,
                    WFHDaysCount = wfhDays,
                    TotalOfficeHours = Math.Round(totalHours, 1),
                    AverageOfficeHoursPerDay = officeDays > 0 ? Math.Round(totalHours / officeDays, 1) : 0.0,
                    ComplianceStatus = officeDays >= 3 ? "MET (3/3 Days)" : (officeDays == 2 ? "PARTIAL (2/3 Days)" : "NON-COMPLIANT (0-1 Days)")
                },
                DailyHistory = recentDaily.Select(d => new
                {
                    Date = d.AttendanceDate.ToString("yyyy-MM-dd"),
                    DayOfWeek = d.AttendanceDate.DayOfWeek.ToString(),
                    Type = d.AttendanceType.ToString(),
                    FirstSeen = d.FirstSeenTime?.ToString("hh:mm tt") ?? "N/A",
                    LastSeen = d.LastSeenTime?.ToString("hh:mm tt") ?? "N/A",
                    Hours = d.TotalOfficeHours,
                    OfficeLocation = d.OfficeLocation?.Name ?? "Remote / Home Network",
                    IsCompliant = d.IsHybridCompliant
                })
            });
        }
    }

    // =====================================================================
    // MANAGER CONTROLLER — Self-scoping to logged-in user's reporting tree
    // =====================================================================
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ManagerOrAbove")]
    public class ManagerController : ControllerBase
    {
        private readonly AttendanceDbContext _context;
        private readonly IOrgHierarchyService _orgHierarchyService;
        private readonly IEmailNotificationService _emailService;

        public ManagerController(AttendanceDbContext context, IOrgHierarchyService orgHierarchyService, IEmailNotificationService emailService)
        {
            _context = context;
            _orgHierarchyService = orgHierarchyService;
            _emailService = emailService;
        }

        /// <summary>
        /// Get direct reports for the currently logged-in manager (self-scoped via SSO/Cookie claims)
        /// </summary>
        [HttpGet("me/direct-reports")]
        public async Task<IActionResult> GetMyDirectReports()
        {
            var managerId = GetCurrentEmployeeId();
            if (managerId == null) return Unauthorized();
            return await GetDirectReportsInternal(managerId.Value);
        }

        /// <summary>
        /// Get direct reports for a specific manager (used by Admin and for drilling into sub-managers)
        /// </summary>
        [HttpGet("{managerId}/direct-reports")]
        public async Task<IActionResult> GetDirectReports(int managerId)
        {
            // For managers: verify the target is in their subtree
            var currentId = GetCurrentEmployeeId();
            if (currentId == null) return Unauthorized();

            var currentRole = User.FindFirst("AppRole")?.Value;
            if (currentRole != "Administrator" && currentRole != "PowerUser")
            {
                var isInSubtree = await _orgHierarchyService.IsEmployeeInManagerSubtreeAsync(currentId.Value, managerId);
                if (!isInSubtree && currentId.Value != managerId)
                    return Forbid();
            }

            return await GetDirectReportsInternal(managerId);
        }

        /// <summary>
        /// Get org chart for the currently logged-in manager
        /// </summary>
        [HttpGet("me/org-chart")]
        public async Task<IActionResult> GetMyOrgChart()
        {
            var managerId = GetCurrentEmployeeId();
            if (managerId == null) return Unauthorized();

            var tree = await _orgHierarchyService.GetOrgChartTreeAsync(managerId.Value);
            return Ok(tree);
        }

        [HttpGet("{managerId}/org-chart")]
        public async Task<IActionResult> GetOrgChart(int managerId)
        {
            var tree = await _orgHierarchyService.GetOrgChartTreeAsync(managerId);
            return Ok(tree);
        }

        /// <summary>
        /// Trigger weekly email preview (stored in DB — no real emails sent)
        /// </summary>
        [HttpPost("me/trigger-weekly-email")]
        public async Task<IActionResult> TriggerWeeklyEmail()
        {
            var managerId = GetCurrentEmployeeId();
            if (managerId == null) return Unauthorized();

            var start = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            await _emailService.SendWeeklyManagerReportAsync(managerId.Value, start);

            var latestLog = await _context.EmailNotificationLogs
                .Where(l => l.RecipientEmployeeId == managerId.Value)
                .OrderByDescending(l => l.SentAt)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                message = "Weekly Attendance Email summary generated in Database Preview Inbox.",
                recipient = latestLog?.RecipientEmail ?? "preview@bkrangroup.com",
                subject = latestLog?.Subject ?? "Weekly Attendance Report",
                bodyHtml = latestLog?.BodyHtml ?? "<h1>Email Content</h1>"
            });
        }

        // ---- LEGACY ENDPOINTS (backward compatible with explicit managerId) ----

        [HttpPost("{managerId}/trigger-weekly-email")]
        public async Task<IActionResult> TriggerWeeklyEmailLegacy(int managerId)
        {
            var start = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            await _emailService.SendWeeklyManagerReportAsync(managerId, start);

            var latestLog = await _context.EmailNotificationLogs
                .Where(l => l.RecipientEmployeeId == managerId)
                .OrderByDescending(l => l.SentAt)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                message = "Weekly Attendance Email summary generated in Database Preview Inbox.",
                recipient = latestLog?.RecipientEmail ?? "preview@bkrangroup.com",
                subject = latestLog?.Subject ?? "Weekly Attendance Report",
                bodyHtml = latestLog?.BodyHtml ?? "<h1>Email Content</h1>"
            });
        }

        // ---- SHARED INTERNAL LOGIC ----

        private async Task<IActionResult> GetDirectReportsInternal(int managerId)
        {
            var directReports = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.OfficeLocation)
                .Where(e => e.ManagerId == managerId && e.IsActive)
                .ToListAsync();

            var start = DateTime.Today.AddDays(-30);
            var empIds = directReports.Select(e => e.Id).ToList();

            var daily = await _context.DailyAttendances
                .Include(d => d.OfficeLocation)
                .Where(d => empIds.Contains(d.EmployeeId) && d.AttendanceDate >= start)
                .ToListAsync();

            var allActiveEmployees = await _context.Employees.Where(e => e.IsActive).ToListAsync();

            var result = directReports.Select(emp =>
            {
                var empDaily = daily.Where(d => d.EmployeeId == emp.Id).OrderByDescending(d => d.AttendanceDate).ToList();
                int officeDays = empDaily.Count(d => d.AttendanceType == AttendanceType.Office);
                int wfhDays = empDaily.Count(d => d.AttendanceType == AttendanceType.WFH);
                double totalHours = empDaily.Sum(d => d.TotalOfficeHours);
                var latestRecord = empDaily.FirstOrDefault();

                bool hasSubordinates = allActiveEmployees.Any(e => e.ManagerId == emp.Id);

                return new
                {
                    emp.Id,
                    emp.FullName,
                    emp.Title,
                    emp.Email,
                    emp.EmployeeCode,
                    Department = emp.Department?.Name,
                    OfficeLocation = emp.OfficeLocation?.Name,
                    OfficeDaysCount = officeDays,
                    WFHDaysCount = wfhDays,
                    TotalOfficeHours = Math.Round(totalHours, 1),
                    FirstSeen = latestRecord?.FirstSeenTime?.ToString("hh:mm tt") ?? "09:15 AM",
                    LastSeen = latestRecord?.LastSeenTime?.ToString("hh:mm tt") ?? "06:15 PM",
                    Status = officeDays >= 12 ? "MET" : "PARTIAL",
                    HasSubordinates = hasSubordinates,
                    DailyHistory = empDaily.Take(10).Select(d => new
                    {
                        Date = d.AttendanceDate.ToString("yyyy-MM-dd"),
                        DayOfWeek = d.AttendanceDate.DayOfWeek.ToString(),
                        Type = d.AttendanceType.ToString(),
                        FirstSeen = d.FirstSeenTime?.ToString("hh:mm tt") ?? "09:15 AM",
                        LastSeen = d.LastSeenTime?.ToString("hh:mm tt") ?? "06:15 PM",
                        Hours = d.TotalOfficeHours,
                        Location = d.OfficeLocation?.Name ?? "Remote / Home Network"
                    })
                };
            });

            return Ok(result);
        }

        private int? GetCurrentEmployeeId()
        {
            var idClaim = User.FindFirst("EmployeeId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            return idClaim != null && int.TryParse(idClaim.Value, out var id) ? id : null;
        }
    }

    // =====================================================================
    // ADMIN CONTROLLER — Full organization scope + Power User management
    // =====================================================================
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOrPowerUser")]
    public class AdminController : ControllerBase
    {
        private readonly AttendanceDbContext _context;
        private readonly IOrgHierarchyService _orgHierarchyService;

        public AdminController(AttendanceDbContext context, IOrgHierarchyService orgHierarchyService)
        {
            _context = context;
            _orgHierarchyService = orgHierarchyService;
        }

        [HttpGet("org-chart-filter")]
        public async Task<IActionResult> GetFilteredOrgChart([FromQuery] string? department, [FromQuery] string? managerName)
        {
            if (!string.IsNullOrWhiteSpace(managerName))
            {
                var mgr = await _context.Employees.FirstOrDefaultAsync(e => e.FullName.Contains(managerName, StringComparison.OrdinalIgnoreCase));
                if (mgr != null)
                {
                    var tree = await _orgHierarchyService.GetOrgChartTreeAsync(mgr.Id);
                    return Ok(tree);
                }
            }

            var defaultTree = await _orgHierarchyService.GetOrgChartTreeAsync(1);

            if (!string.IsNullOrWhiteSpace(department) && department != "ALL" && defaultTree != null)
            {
                FilterDtoByDepartment(defaultTree, department);
            }

            return Ok(defaultTree);
        }

        private bool FilterDtoByDepartment(OrgNodeDto node, string department)
        {
            node.DirectReports = node.DirectReports.Where(child => FilterDtoByDepartment(child, department)).ToList();
            return node.Department.Equals(department, StringComparison.OrdinalIgnoreCase) || node.DirectReports.Any();
        }

        [HttpGet("direct-reports/{managerId}")]
        public async Task<IActionResult> GetDirectReports(int managerId)
        {
            var directReports = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.OfficeLocation)
                .Where(e => e.ManagerId == managerId && e.IsActive)
                .ToListAsync();

            var start = DateTime.Today.AddDays(-30);
            var empIds = directReports.Select(e => e.Id).ToList();

            var daily = await _context.DailyAttendances
                .Include(d => d.OfficeLocation)
                .Where(d => empIds.Contains(d.EmployeeId) && d.AttendanceDate >= start)
                .ToListAsync();

            var allActiveEmployees = await _context.Employees.Where(e => e.IsActive).ToListAsync();

            var result = directReports.Select(emp =>
            {
                var empDaily = daily.Where(d => d.EmployeeId == emp.Id).OrderByDescending(d => d.AttendanceDate).ToList();
                int officeDays = empDaily.Count(d => d.AttendanceType == AttendanceType.Office);
                int wfhDays = empDaily.Count(d => d.AttendanceType == AttendanceType.WFH);
                double totalHours = empDaily.Sum(d => d.TotalOfficeHours);
                var latestRecord = empDaily.FirstOrDefault();

                bool hasSubordinates = allActiveEmployees.Any(e => e.ManagerId == emp.Id);

                return new
                {
                    emp.Id,
                    emp.FullName,
                    emp.Title,
                    emp.Email,
                    emp.EmployeeCode,
                    Department = emp.Department?.Name,
                    OfficeLocation = emp.OfficeLocation?.Name,
                    OfficeDaysCount = officeDays,
                    WFHDaysCount = wfhDays,
                    TotalOfficeHours = Math.Round(totalHours, 1),
                    FirstSeen = latestRecord?.FirstSeenTime?.ToString("hh:mm tt") ?? "09:15 AM",
                    LastSeen = latestRecord?.LastSeenTime?.ToString("hh:mm tt") ?? "06:15 PM",
                    Status = officeDays >= 12 ? "MET" : "PARTIAL",
                    HasSubordinates = hasSubordinates
                };
            });

            return Ok(result);
        }

        [HttpGet("all-employees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.OfficeLocation)
                .Include(e => e.Manager)
                .Where(e => e.IsActive)
                .ToListAsync();

            var start = DateTime.Today.AddDays(-30);

            var daily = await _context.DailyAttendances
                .Where(d => d.AttendanceDate >= start)
                .ToListAsync();

            var result = employees.Select(emp =>
            {
                var empDaily = daily.Where(d => d.EmployeeId == emp.Id).ToList();
                int officeDays = empDaily.Count(d => d.AttendanceType == AttendanceType.Office);
                int wfhDays = empDaily.Count(d => d.AttendanceType == AttendanceType.WFH);
                double totalHours = empDaily.Sum(d => d.TotalOfficeHours);

                var latestRecord = empDaily.OrderByDescending(d => d.AttendanceDate).FirstOrDefault();

                return new
                {
                    emp.Id,
                    emp.FullName,
                    emp.Title,
                    emp.Email,
                    Department = emp.Department?.Name,
                    OfficeLocation = emp.OfficeLocation?.Name,
                    ManagerName = emp.Manager?.FullName ?? "Executive Leadership",
                    OfficeDays = officeDays,
                    WFHDays = wfhDays,
                    TotalOfficeHours = Math.Round(totalHours, 1),
                    FirstSeen = latestRecord?.FirstSeenTime?.ToString("hh:mm tt") ?? "09:15 AM",
                    LastSeen = latestRecord?.LastSeenTime?.ToString("hh:mm tt") ?? "06:15 PM",
                    Status = officeDays >= 12 ? "MET" : "PARTIAL"
                };
            });

            return Ok(result);
        }

        // ===== POWER USER MANAGEMENT (Admin-only) =====

        /// <summary>
        /// Grant Power User access (read-only admin dashboard) to an employee by email
        /// </summary>
        [HttpPost("power-users")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GrantPowerUserAccess([FromBody] PowerUserRequest request)
        {
            var isPowerUser = User.FindFirst("AppRole")?.Value;
            if (isPowerUser == "PowerUser")
                return Forbid();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

            if (employee == null)
                return NotFound(new { message = $"Employee with email '{request.Email}' not found." });

            employee.Role = UserRole.PowerUser;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"{employee.FullName} has been granted Power User access (read-only admin dashboard).",
                employee = new { employee.Id, employee.FullName, employee.Email, Role = employee.Role.ToString() }
            });
        }

        /// <summary>
        /// Revoke Power User access from an employee
        /// </summary>
        [HttpDelete("power-users/{email}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> RevokePowerUserAccess(string email)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (employee == null)
                return NotFound(new { message = $"Employee with email '{email}' not found." });

            // Revert to Manager role (default for non-admin)
            employee.Role = UserRole.Manager;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Power User access revoked from {employee.FullName}." });
        }

        /// <summary>
        /// List all current Power Users
        /// </summary>
        [HttpGet("power-users")]
        public async Task<IActionResult> GetPowerUsers()
        {
            var powerUsers = await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.Role == UserRole.PowerUser && e.IsActive)
                .ToListAsync();

            return Ok(powerUsers.Select(e => new
            {
                e.Id,
                e.FullName,
                e.Email,
                e.Title,
                Department = e.Department?.Name,
                Role = e.Role.ToString()
            }));
        }

        /// <summary>
        /// Check if current user is a Power User (read-only) or full Admin
        /// </summary>
        [HttpGet("access-level")]
        public IActionResult GetAccessLevel()
        {
            var role = User.FindFirst("AppRole")?.Value ?? "Unknown";
            return Ok(new
            {
                Role = role,
                IsReadOnly = role == "PowerUser",
                CanModifyConfig = role == "Administrator",
                CanManagePowerUsers = role == "Administrator"
            });
        }
    }

    public class PowerUserRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    // =====================================================================
    // REPORTS CONTROLLER — Excel report generation
    // =====================================================================
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ManagerOrAbove")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportGenerator _reportGenerator;

        public ReportsController(IReportGenerator reportGenerator)
        {
            _reportGenerator = reportGenerator;
        }

        [HttpGet("weekly-excel/{managerId}")]
        public async Task<IActionResult> DownloadWeeklyExcel(int managerId, [FromQuery] DateTime? weekStartDate)
        {
            var start = weekStartDate?.Date ?? DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            var bytes = await _reportGenerator.GenerateWeeklyManagerExcelReportAsync(managerId, start);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Weekly_Attendance_Report_{start:yyyyMMdd}.xlsx");
        }
    }
}
