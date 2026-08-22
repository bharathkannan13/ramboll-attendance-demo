using System;
using System.Collections.Generic;
using EnterpriseAttendance.Core.Enums;

namespace EnterpriseAttendance.Core.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public string EntraObjectId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        public int? ManagerId { get; set; }
        public Employee? Manager { get; set; }
        public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

        public int? OfficeLocationId { get; set; }
        public OfficeLocation? OfficeLocation { get; set; }

        public UserRole Role { get; set; } = UserRole.Manager;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public ICollection<AttendanceSession> AttendanceSessions { get; set; } = new List<AttendanceSession>();
        public ICollection<DailyAttendance> DailyAttendances { get; set; } = new List<DailyAttendance>();
        public ICollection<AttendanceSummary> AttendanceSummaries { get; set; } = new List<AttendanceSummary>();
    }

    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int? DepartmentHeadId { get; set; }
        public Employee? DepartmentHead { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }

    public class OfficeLocation
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g. Ramboll Chennai
        public string City { get; set; } = string.Empty; // Chennai, Noida, Hyderabad, Gurugram, Bangalore
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "India";
        public string TimeZone { get; set; } = "India Standard Time";
        public bool IsActive { get; set; } = true;

        public ICollection<OfficeNetwork> OfficeNetworks { get; set; } = new List<OfficeNetwork>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }

    public class OfficeNetwork
    {
        public int Id { get; set; }
        public int OfficeLocationId { get; set; }
        public OfficeLocation? OfficeLocation { get; set; }

        public NetworkType NetworkType { get; set; }
        public string NetworkValue { get; set; } = string.Empty; // e.g. "Ramboll-CHN" or "10.100.0.0/16"
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Device
    {
        public int Id { get; set; }
        public string IntuneDeviceId { get; set; } = string.Empty;
        public string? DefenderDeviceId { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public string OperatingSystem { get; set; } = string.Empty;
        public string OSVersion { get; set; } = string.Empty;
        public ComplianceStatus ComplianceStatus { get; set; } = ComplianceStatus.Compliant;
        public bool IsManaged { get; set; } = true;
        public DateTime LastSyncTime { get; set; } = DateTime.UtcNow;
        public DateTime? RetiredAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<AttendanceSession> Sessions { get; set; } = new List<AttendanceSession>();
    }

    public class AttendanceSession
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public int? DeviceId { get; set; }
        public Device? Device { get; set; }

        public DateTime SessionDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastSeenTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double DurationMinutes { get; set; }

        public NetworkLocationType NetworkLocationType { get; set; }
        public int? OfficeLocationId { get; set; }
        public OfficeLocation? OfficeLocation { get; set; }

        public int? MatchedNetworkId { get; set; }
        public OfficeNetwork? MatchedNetwork { get; set; }

        public string IPAddress { get; set; } = string.Empty;
        public string DetectedSSID { get; set; } = string.Empty;

        public SessionStatus SessionStatus { get; set; } = SessionStatus.Active;
        public CloseReason? ClosedReason { get; set; }
        public double ConfidenceScore { get; set; } = 1.0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class DailyAttendance
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime AttendanceDate { get; set; }
        public int? OfficeLocationId { get; set; }
        public OfficeLocation? OfficeLocation { get; set; }

        public AttendanceType AttendanceType { get; set; } = AttendanceType.Office;
        public DateTime? FirstSeenTime { get; set; }
        public DateTime? LastSeenTime { get; set; }
        public double TotalOfficeHours { get; set; }
        public int TotalSessions { get; set; }

        public NetworkLocationType PrimaryNetworkType { get; set; }
        public bool IsHybridCompliant { get; set; }
        public double ConfidenceScore { get; set; } = 1.0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public class AttendanceSummary
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        public PeriodType PeriodType { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }

        public int TotalWorkingDays { get; set; }
        public int OfficeDaysCount { get; set; }
        public int WFHDaysCount { get; set; }
        public int AbsentDaysCount { get; set; }

        public double TotalOfficeHours { get; set; }
        public double AverageOfficeHoursPerDay { get; set; }
        public double HybridCompliancePercentage { get; set; }
        public int PolicyTargetOfficeDays { get; set; } = 3;
        public PolicyComplianceStatus PolicyComplianceStatus { get; set; } = PolicyComplianceStatus.Met;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class TelemetryEvent
    {
        public int Id { get; set; }
        public string EventId { get; set; } = Guid.NewGuid().ToString();

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public int? DeviceId { get; set; }
        public Device? Device { get; set; }

        public TelemetrySource TelemetrySource { get; set; }
        public EventType EventType { get; set; }
        public DateTime Timestamp { get; set; }

        public string IPAddress { get; set; } = string.Empty;
        public string NetworkSSID { get; set; } = string.Empty;
        public string SubnetInfo { get; set; } = string.Empty;
        public string RawPayloadJson { get; set; } = string.Empty;

        public bool IsDuplicate { get; set; } = false;
        public bool ProcessedStatus { get; set; } = false;
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class BusinessRule
    {
        public int Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string RuleKey { get; set; } = string.Empty;
        public string RuleValue { get; set; } = string.Empty;
        public string DataType { get; set; } = "System.String";
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string UserEmail { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string Severity { get; set; } = "Information";
    }

    public class ApiLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Endpoint { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public int ResponseCode { get; set; }
        public double DurationMs { get; set; }
        public string TelemetrySource { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class EmailNotificationLog
    {
        public int Id { get; set; }
        public int RecipientEmployeeId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty; // WeeklyReport, MonthlySummary
        public string Subject { get; set; } = string.Empty;
        public string BodyHtml { get; set; } = string.Empty;
        public string AttachmentPath { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string DeliveryStatus { get; set; } = "Sent"; // Sent, Failed, PreviewInbox
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class EmailTemplate
    {
        public int Id { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateType { get; set; } = string.Empty; // WeeklyManagerReport, MonthlyManagerSummary
        public string SubjectTemplate { get; set; } = string.Empty;
        public string BodyTemplate { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class WeeklyReportLog
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public int DirectReportCount { get; set; }
        public double AverageAttendancePct { get; set; }
        public string ReportHtmlContent { get; set; } = string.Empty;
        public string DeliveryStatus { get; set; } = "Sent";
    }

    public class SystemConfiguration
    {
        public int Id { get; set; }
        public string ConfigKey { get; set; } = string.Empty;
        public string ConfigValue { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEncrypted { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
