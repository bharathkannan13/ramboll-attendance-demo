using Microsoft.AspNetCore.Mvc;
using RambollAttendanceAPI.Models;
using RambollAttendanceAPI.Repositories;
using System;
using System.Threading.Tasks;

namespace RambollAttendanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly IAttendanceRepository _repository;

        public TelemetryController(IAttendanceRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("log")]
        public async Task<IActionResult> LogTelemetry([FromBody] TelemetryEvent requestEvent)
        {
            // 1. Authenticate via SSO Header
            if (!Request.Headers.TryGetValue("X-SSO-Identity", out var ssoIdentity) || string.IsNullOrEmpty(ssoIdentity))
            {
                return Unauthorized("Missing X-SSO-Identity authorization header.");
            }

            try
            {
                // 2. Fetch employee matching SSO Identity
                var employee = await _repository.GetEmployeeBySSOAsync(ssoIdentity.ToString());
                if (employee == null)
                {
                    // Auto-register unknown SSO users on-the-fly for demo convenience
                    string fullName = Request.Headers.TryGetValue("X-Employee-Name", out var nameHeader) 
                        ? nameHeader.ToString() 
                        : ssoIdentity.ToString().Split('\\').Last();
                    
                    // Capitalize first letter of fallback username for presentation
                    if (!Request.Headers.ContainsKey("X-Employee-Name"))
                    {
                        fullName = char.ToUpper(fullName[0]) + fullName.Substring(1);
                    }

                    string email = ssoIdentity.ToString().Contains('@') 
                        ? ssoIdentity.ToString() 
                        : $"{fullName.Replace(" ", ".").ToLower()}@ramboll.com";

                    employee = await _repository.RegisterEmployeeAsync(
                        ssoIdentity.ToString(),
                        fullName,
                        email,
                        "Demo Sandbox"
                    );
                }

                // 3. Log Raw Telemetry to Database
                DateTime logTime = requestEvent.Timestamp == default ? DateTime.Now : requestEvent.Timestamp;
                await _repository.LogTelemetryAsync(
                    employee.Employee_ID,
                    requestEvent.MAC_Address,
                    requestEvent.Hostname,
                    requestEvent.Event_Type,
                    logTime,
                    requestEvent.SSID
                );

                // 4. Trigger Real-Time Aggregation for today's summary
                await _repository.RecalculateDailySummaryAsync(employee.Employee_ID, logTime.Date);

                return Ok(new
                {
                    Message = "Telemetry log processed and aggregated.",
                    Employee = employee.FullName,
                    Event = requestEvent.Event_Type,
                    Time = logTime
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
