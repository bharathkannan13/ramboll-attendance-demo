using Microsoft.AspNetCore.Mvc;
using RambollAttendanceAPI.Models;
using RambollAttendanceAPI.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RambollAttendanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IAttendanceRepository _repository;

        public ReportsController(IAttendanceRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardSummary([FromQuery] string? date)
        {
            if (!Request.Headers.TryGetValue("X-SSO-Identity", out var ssoIdentity) || string.IsNullOrEmpty(ssoIdentity))
            {
                return Unauthorized("Missing X-SSO-Identity header.");
            }

            DateTime queryDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

            try
            {
                var employee = await _repository.GetEmployeeBySSOAsync(ssoIdentity.ToString());
                if (employee == null) return NotFound("Employee not found.");

                var summaries = await _repository.GetDailySummariesAsync(queryDate);
                var userSummary = summaries.FirstOrDefault(s => s.Employee_ID == employee.Employee_ID);

                // If no summary exists yet for today, send a mock/default state
                userSummary ??= new AttendanceSummary
                {
                    Employee_ID = employee.Employee_ID,
                    FullName = employee.FullName,
                    Department = employee.Department,
                    SSO_Identity = employee.SSO_Identity,
                    Work_Date = queryDate,
                    First_Seen = null,
                    Last_Seen = null,
                    Active_Hours = 0.00m,
                    Status = "ABSENT"
                };

                return Ok(userSummary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetDailyHistory([FromQuery] string? date)
        {
            if (!Request.Headers.TryGetValue("X-SSO-Identity", out var ssoIdentity) || string.IsNullOrEmpty(ssoIdentity))
            {
                return Unauthorized("Missing X-SSO-Identity header.");
            }

            DateTime queryDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

            try
            {
                var employee = await _repository.GetEmployeeBySSOAsync(ssoIdentity.ToString());
                if (employee == null) return NotFound("Employee not found.");

                var logs = await _repository.GetTelemetryLogsAsync(employee.Employee_ID, queryDate);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("history/all")]
        public async Task<IActionResult> GetAllDailyHistory([FromQuery] string? date)
        {
            DateTime queryDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

            try
            {
                var logs = await _repository.GetAllTelemetryLogsAsync(queryDate);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDailySummaryList([FromQuery] string? date)
        {
            DateTime queryDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

            try
            {
                var summaries = await _repository.GetDailySummariesAsync(queryDate);
                return Ok(summaries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetDemoData()
        {
            try
            {
                await _repository.ClearAllTelemetryAndSummariesAsync();
                return Ok(new { Message = "Demo telemetry logs and daily summaries cleared successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportToCSV([FromQuery] string? date)
        {
            DateTime queryDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

            try
            {
                var summaries = await _repository.GetDailySummariesAsync(queryDate);
                
                var csvBuilder = new StringBuilder();
                // Write Header
                csvBuilder.AppendLine("Employee ID,Full Name,SSO Identity,Department,Work Date,First Seen,Last Seen,Active Hours,Status");

                foreach (var summary in summaries)
                {
                    string firstSeenStr = summary.First_Seen?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
                    string lastSeenStr = summary.Last_Seen?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";

                    csvBuilder.AppendLine(
                        $"{summary.Employee_ID}," +
                        $"\"{summary.FullName}\"," +
                        $"\"{summary.SSO_Identity}\"," +
                        $"\"{summary.Department}\"," +
                        $"{summary.Work_Date:yyyy-MM-dd}," +
                        $"{firstSeenStr}," +
                        $"{lastSeenStr}," +
                        $"{summary.Active_Hours:0.00}," +
                        $"{summary.Status}"
                    );
                }

                byte[] bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                string fileName = $"Ramboll_Attendance_{queryDate:yyyyMMdd}.csv";

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Export failed: {ex.Message}");
            }
        }
    }
}
