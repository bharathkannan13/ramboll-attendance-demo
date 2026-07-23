using System;

namespace RambollAttendanceAPI.Models
{
    public class Device
    {
        public int Device_ID { get; set; }
        public string MAC_Address { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
        public int Employee_ID { get; set; }
        public DateTime Last_Seen { get; set; }
        public DateTime Created_At { get; set; }
    }
}
