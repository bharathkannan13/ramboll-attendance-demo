using System;

namespace RambollAttendanceAPI.Models
{
    public class TelemetryEvent
    {
        public string Hostname { get; set; } = string.Empty;
        public string MAC_Address { get; set; } = string.Empty;
        public string Event_Type { get; set; } = string.Empty; // 'WAKE', 'SLEEP', 'LOCK', 'UNLOCK', 'HEARTBEAT', 'DISCONNECT'
        public string SSID { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
