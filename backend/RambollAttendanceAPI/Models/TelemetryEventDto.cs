using System;

namespace RambollAttendanceAPI.Models
{
    public class TelemetryEventDto
    {
        public DateTime Timestamp { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
        public string MAC_Address { get; set; } = string.Empty;
        public string Event_Type { get; set; } = string.Empty;
        public string SSID { get; set; } = string.Empty;
    }
}
