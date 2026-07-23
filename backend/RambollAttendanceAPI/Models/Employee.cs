using System;

namespace RambollAttendanceAPI.Models
{
    public class Employee
    {
        public int Employee_ID { get; set; }
        public string SSO_Identity { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public bool Is_Active { get; set; }
        public DateTime Created_At { get; set; }
    }
}
