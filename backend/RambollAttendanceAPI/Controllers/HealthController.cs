using Microsoft.AspNetCore.Mvc;
using RambollAttendanceAPI.Data;
using System;
using System.Data;

namespace RambollAttendanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public HealthController(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        [HttpGet]
        public IActionResult GetHealth()
        {
            try
            {
                using var conn = _connectionFactory.CreateConnection();
                conn.Open();
                
                return Ok(new
                {
                    Status = "Healthy",
                    Database = "Connected",
                    Timestamp = DateTime.Now,
                    Environment = "Local Development (POC)"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Unhealthy",
                    Database = "Disconnected",
                    Error = ex.Message,
                    Timestamp = DateTime.Now
                });
            }
        }
    }
}
