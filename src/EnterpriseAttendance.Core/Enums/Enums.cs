namespace EnterpriseAttendance.Core.Enums
{
    public enum NetworkLocationType
    {
        CorporateOffice = 1, // Corporate Office Network (matched via subnet/SSID)
        Remote = 2,
        VPN = 3,
        Unknown = 4
    }

    public enum AttendanceType
    {
        Office = 1,
        WFH = 2,
        Absent = 3,
        Holiday = 4,
        Weekend = 5,
        Leave = 6
    }

    public enum SessionStatus
    {
        Active = 1,
        Closed = 2,
        Merged = 3
    }

    public enum CloseReason
    {
        Sleep = 1,
        Hibernate = 2,
        Shutdown = 3,
        Disconnect = 4,
        NetworkLeave = 5,
        GracePeriodExpiry = 6,
        EndOfDay = 7,
        Manual = 8
    }

    public enum ComplianceStatus
    {
        Compliant = 1,
        NonCompliant = 2,
        Unknown = 3
    }

    public enum PolicyComplianceStatus
    {
        Met = 1,
        PartiallyMet = 2,
        NonCompliant = 3
    }

    public enum TelemetrySource
    {
        EntraID = 1,
        Intune = 2,
        Defender = 3
    }

    public enum EventType
    {
        Logon = 1,
        Heartbeat = 2,
        NetworkChange = 3,
        DeviceSync = 4,
        SleepState = 5
    }

    public enum NetworkType
    {
        SSID = 1,
        Subnet = 2,
        IPRange = 3,
        VLAN = 4,
        VPN = 5,
        NAC = 6
    }

    public enum UserRole
    {
        Administrator = 1,
        Manager = 2,
        PowerUser = 3  // Read-only access to Admin Dashboard (created by Admin)
    }

    public enum PeriodType
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3
    }
}
