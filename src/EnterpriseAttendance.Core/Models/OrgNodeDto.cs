using System.Collections.Generic;

namespace EnterpriseAttendance.Core.Models
{
    public class OrgNodeDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string OfficeLocation { get; set; } = string.Empty;
        public int? ManagerId { get; set; }
        public int OfficeDays { get; set; } = 3;
        public string ComplianceStatus { get; set; } = "MET";
        public List<OrgNodeDto> DirectReports { get; set; } = new List<OrgNodeDto>();
    }
}
