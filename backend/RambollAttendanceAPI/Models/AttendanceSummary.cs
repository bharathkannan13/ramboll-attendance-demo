using System;

namespace RambollAttendanceAPI.Models
{
    public class AttendanceSummary
    {
        public int Summary_ID { get; set; }
        public int Employee_ID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string SSO_Identity { get; set; } = string.Empty;
        public DateTime Work_Date { get; set; }
        public DateTime? First_Seen { get; set; }
        public DateTime? Last_Seen { get; set; }
        public decimal Active_Hours { get; set; }
        public string Status { get; set; } = "ABSENT";
    }
}
