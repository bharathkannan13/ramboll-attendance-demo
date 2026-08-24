using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterpriseAttendance.Infrastructure.Data;

namespace EnterpriseAttendance.Web.Controllers
{
    public class AdminViewController : Controller
    {
        [HttpGet("/Admin")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Index.cshtml");
        }
    }

    public class ManagerViewController : Controller
    {
        [HttpGet("/Manager")]
        public IActionResult Index()
        {
            return View("~/Views/Manager/Index.cshtml");
        }
    }

    [AllowAnonymous]
    public class AuthViewController : Controller
    {
        private readonly AttendanceDbContext _context;
        private readonly IConfiguration _config;

        public AuthViewController(AttendanceDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("/Auth/Login")]
        [HttpGet("/")]
        public async Task<IActionResult> Login([FromQuery] bool logout = false)
        {
            if (logout)
            {
                try { await HttpContext.SignOutAsync("DemoCookies"); } catch { }
                try { await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); } catch { }
                
                Response.Cookies.Delete(".AspNetCore.DemoCookies");
                Response.Cookies.Delete(".AspNetCore.Cookies");
                Response.Cookies.Delete("DemoCookies");
            }

            ViewBag.UseMockTelemetry = _config.GetValue<bool>("TelemetrySettings:UseMockTelemetry", true);
            return View("~/Views/Auth/Login.cshtml");
        }

        [HttpGet("/Auth/Logout")]
        [HttpPost("/Auth/Logout")]
        public async Task<IActionResult> Logout()
        {
            try { await HttpContext.SignOutAsync("DemoCookies"); } catch { }
            try { await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); } catch { }

            try
            {
                Response.Cookies.Delete(".AspNetCore.DemoCookies");
                Response.Cookies.Delete(".AspNetCore.Cookies");
                Response.Cookies.Delete("DemoCookies");
            }
            catch { }

            return Redirect("/Auth/Login?logout=true");
        }

        /// <summary>
        /// SSO Callback: After Entra ID SSO login, look up user email and establish session
        /// </summary>
        [HttpGet("/Auth/SsoCallback")]
        [Authorize]
        public async Task<IActionResult> SsoCallback()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value
                      ?? User.FindFirst("preferred_username")?.Value
                      ?? User.FindFirst("upn")?.Value;

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (employee == null)
            {
                ViewBag.Error = "Your email is not registered in the system. Please contact your administrator.";
                return View("~/Views/Auth/Login.cshtml");
            }

            var identity = User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                identity.AddClaim(new Claim("AppRole", employee.Role.ToString()));
                identity.AddClaim(new Claim("EmployeeId", employee.Id.ToString()));
            }

            if (employee.Role == Core.Enums.UserRole.Administrator || employee.Role == Core.Enums.UserRole.PowerUser)
                return Redirect("/Admin");

            return Redirect("/Manager");
        }
    }
}
