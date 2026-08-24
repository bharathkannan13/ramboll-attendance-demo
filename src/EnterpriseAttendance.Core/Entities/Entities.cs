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
        public string City { get; set; } = string.Empty; // Chennai, Noida, Hyderabad, Gurugram, Bangalore, Mumbai, Pune, Delhi
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
        public string NetworkValue { get; set; } = string.Empty;
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
        public string NotificationType { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string BodyHtml { get; set; } = string.Empty;
        public string AttachmentPath { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string DeliveryStatus { get; set; } = "Sent";
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class EmailTemplate
    {
        public int Id { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateType { get; set; } = string.Empty;
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

    // =====================================================================
    // 18-TABLE ENTERPRISE SPECIFICATION ENTITIES
    // =====================================================================

    // Table 1: Application_Ownership
    public class ApplicationOwnership
    {
        public int Application_ID { get; set; }
        public string Application_Name { get; set; } = "Bkran Group Connect Attendance Portal";
        public string Business_Owner { get; set; } = "Human Resources (HR)";
        public string Technical_Owner { get; set; } = "Cyber Security & Cloud Infrastructure";
        public string Support_Group { get; set; } = "Global IT Service Desk";
        public string Escalation_Email { get; set; } = "support-attendance@bkrangroup.com";
        public DateTime Created_Date { get; set; } = DateTime.UtcNow;
    }

    // Table 2: Role_Master
    public class RoleMaster
    {
        public int Role_ID { get; set; }
        public string Role_Name { get; set; } = string.Empty; // Admin, HR, Manager, Employee, Security Analyst
        public string Description { get; set; } = string.Empty;
        public bool Is_Active { get; set; } = true;
        public DateTime Created_Date { get; set; } = DateTime.UtcNow;
    }

    // Table 3: User_Role
    public class UserRoleEntity
    {
        public int User_Role_ID { get; set; }
        public int Employee_ID { get; set; }
        public Employee? Employee { get; set; }
        public int Role_ID { get; set; }
        public RoleMaster? Role { get; set; }
        public DateTime Assigned_Date { get; set; } = DateTime.UtcNow;
    }

    // Table 4: Permission_Master
    public class PermissionMaster
    {
        public int Permission_ID { get; set; }
        public string Permission_Name { get; set; } = string.Empty; // Attendance_Edit, Report_Export, Role_Change, System_Config
        public string Description { get; set; } = string.Empty;
    }

    // Table 5: Role_Permission
    public class RolePermission
    {
        public int Id { get; set; }
        public int Role_ID { get; set; }
        public RoleMaster? Role { get; set; }
        public int Permission_ID { get; set; }
        public PermissionMaster? Permission { get; set; }
    }

    // Table 6: Security_Audit_Log
    public class SecurityAuditLog
    {
        public int Audit_ID { get; set; }
        public int Employee_ID { get; set; }
        public string Employee_Name { get; set; } = string.Empty;
        public string Action_Type { get; set; } = string.Empty; // Login, Logout, Attendance Edit, Report Export, Role Changes
        public DateTime Action_Time { get; set; } = DateTime.UtcNow;
        public string IP_Address { get; set; } = string.Empty;
        public string Device_Name { get; set; } = string.Empty;
        public string Result { get; set; } = "SUCCESS";
        public string Remarks { get; set; } = string.Empty;
    }

    // Table 7: Login_Session_Log
    public class LoginSessionLog
    {
        public int Session_ID { get; set; }
        public int Employee_ID { get; set; }
        public DateTime Login_Time { get; set; } = DateTime.UtcNow;
        public DateTime? Logout_Time { get; set; }
        public string IP_Address { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
        public string Device_Name { get; set; } = string.Empty;
        public string Session_Status { get; set; } = "ACTIVE"; // ACTIVE, TERMINATED, EXPIRED
    }

    // Table 8: Error_Log
    public class ErrorLog
    {
        public int Error_ID { get; set; }
        public string Error_Source { get; set; } = string.Empty;
        public string Error_Message { get; set; } = string.Empty;
        public string Stack_Trace { get; set; } = string.Empty;
        public DateTime Created_Time { get; set; } = DateTime.UtcNow;
        public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
        public string Status { get; set; } = "UNRESOLVED";
    }

    // Table 9: Office_Master
    public class OfficeMaster
    {
        public int Office_ID { get; set; }
        public string Office_Name { get; set; } = string.Empty; // Chennai, Bangalore, Mumbai, Pune, Delhi, Noida, Hyderabad, Gurugram
        public string Country { get; set; } = "India";
        public string City { get; set; } = string.Empty;
        public string Timezone { get; set; } = "IST";
        public bool Is_Active { get; set; } = true;
    }

    // Table 10: Work_Mode_Master
    public class WorkModeMaster
    {
        public int Mode_ID { get; set; }
        public string Mode_Name { get; set; } = string.Empty; // Office, WFH, Client Site, Travel
        public string Description { get; set; } = string.Empty;
    }

    // Table 11: Retention_Config
    public class RetentionConfig
    {
        public int Config_ID { get; set; }
        public string Table_Name { get; set; } = string.Empty; // Attendance (2555 days), Audit (3650 days), Error Logs (365 days)
        public int Retention_Days { get; set; }
        public bool Archive_Flag { get; set; } = true;
    }

    // Table 12: Backup_Log
    public class BackupLog
    {
        public int Backup_ID { get; set; }
        public string Backup_Type { get; set; } = "Daily"; // Daily, Weekly, Monthly
        public DateTime Backup_Date { get; set; } = DateTime.UtcNow;
        public string Backup_Status { get; set; } = "COMPLETED";
        public string Backup_Location { get; set; } = "Azure Blob Storage Secondary Vault";
    }

    // Table 13: Api_Access_Log
    public class ApiAccessLog
    {
        public int Access_ID { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public int User_ID { get; set; }
        public DateTime Request_Time { get; set; } = DateTime.UtcNow;
        public int Status_Code { get; set; } = 200;
        public string IP_Address { get; set; } = string.Empty;
    }

    // Table 14: Integration_Config
    public class IntegrationConfig
    {
        public int Integration_ID { get; set; }
        public string System_Name { get; set; } = string.Empty; // Microsoft Entra ID, Workday, Cisco ISE, Intune, Active Directory
        public string Endpoint_URL { get; set; } = string.Empty;
        public string Authentication_Type { get; set; } = "OAuth 2.0 Client Credentials";
        public string Status { get; set; } = "CONNECTED";
    }

    // Table 15: Device_Master
    public class DeviceMaster
    {
        public int Device_ID { get; set; }
        public int Employee_ID { get; set; }
        public Employee? Employee { get; set; }
        public string Hostname { get; set; } = string.Empty;
        public string MAC_Address { get; set; } = string.Empty;
        public string OS_Version { get; set; } = string.Empty;
        public string Serial_Number { get; set; } = string.Empty;
        public DateTime Last_Seen { get; set; } = DateTime.UtcNow;
        public string Compliance_Status { get; set; } = "Compliant"; // Compliant, Non-Compliant, Unknown
    }

    // Table 16: Attendance_Risk_Log
    public class AttendanceRiskLog
    {
        public int Risk_ID { get; set; }
        public int Employee_ID { get; set; }
        public string Employee_Name { get; set; } = string.Empty;
        public string Risk_Type { get; set; } = string.Empty; // Impossible Travel, Multiple Devices, Repeated Failed Login, Suspicious Pattern
        public int Risk_Score { get; set; } = 85; // 0-100
        public DateTime Created_Time { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "FLAGGED"; // FLAGGED, INVESTIGATING, RESOLVED
    }

    // Table 17: Analytics_Log
    public class AnalyticsLog
    {
        public int Prediction_ID { get; set; }
        public string Prediction_Type { get; set; } = string.Empty; // Attendance Trend, Office Occupancy, Peak Login Hours, Absenteeism Pattern
        public string Prediction_Value { get; set; } = string.Empty;
        public DateTime Generated_Time { get; set; } = DateTime.UtcNow;
    }
}
