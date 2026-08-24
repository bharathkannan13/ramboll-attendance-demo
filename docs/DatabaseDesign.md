# Enterprise Database Specification (18-Table Schema)

> **Document ID**: DBD-2026-ENT-003  
> **Target RDBMS**: Microsoft SQL Server / EF Core 8.0  
> **Total Table Count**: 18 Enterprise Tables  

---

## 1. Complete Database Table Catalog

| Table # | Physical Table Name | Primary Key | Description |
|---|---|---|---|
| 1 | `Employee_Master` | `Employee_ID` | Master employee profile, email, job title, and reporting line |
| 2 | `Attendance_Log` | `Attendance_ID` | Daily aggregated attendance, office vs WFH status, net working hours |
| 3 | `Attendance_Audit` | `Audit_ID` | Historical raw telemetry sessions and connect/disconnect timestamps |
| 4 | `Application_Ownership` | `Application_ID` | Governance records mapping business and technical ownership |
| 5 | `Role_Master` | `Role_ID` | Enterprise role definitions (`Admin`, `HR`, `Manager`, `Employee`, `Security Analyst`) |
| 6 | `User_Role` | `User_Role_ID` | Mapping table binding employees to enterprise roles |
| 7 | `Permission_Master` | `Permission_ID` | System permission registry |
| 8 | `Role_Permission` | `Id` | Role to permission mapping table |
| 9 | `Security_Audit_Log` | `Audit_ID` | Audit trail for security actions, role changes, and report exports |
| 10 | `Login_Session_Log` | `Session_ID` | Session tracking logging IP address, browser user-agent, and status |
| 11 | `Error_Log` | `Error_ID` | Centralized error monitoring log with severity classification |
| 12 | `Office_Master` | `Office_ID` | Corporate regional office hub master (Chennai, Bangalore, Mumbai, Pune, Delhi, Noida, Hyderabad, Gurugram) |
| 13 | `Work_Mode_Master` | `Mode_ID` | Authorized work modes (Office, WFH, Client Site, Travel) |
| 14 | `Retention_Config` | `Config_ID` | Retention & archiving policies (Attendance=2555 days, Audit=3650 days) |
| 15 | `Backup_Log` | `Backup_ID` | Automated database backup execution log |
| 16 | `Api_Access_Log` | `Access_ID` | API endpoint access log for rate limiting and security monitoring |
| 17 | `Integration_Config` | `Integration_ID` | External service integration endpoints (Entra ID, Workday, Cisco ISE, Intune, Active Directory) |
| 18 | `Device_Master` | `Device_ID` | Managed laptop hardware inventory (Hostname, MAC address, Serial number, Compliance) |

---

## 2. Cybersecurity & AI Analytics Tables

### `Attendance_Risk_Log`
- **Columns**: `Risk_ID` (PK), `Employee_ID` (FK), `Employee_Name`, `Risk_Type`, `Risk_Score` (0-100), `Created_Time`, `Status`.
- **Purpose**: Flags security anomalies such as Impossible Travel, Multiple Devices, and Suspicious Login Patterns.

### `Analytics_Log`
- **Columns**: `Prediction_ID` (PK), `Prediction_Type`, `Prediction_Value`, `Generated_Time`.
- **Purpose**: Stores AI predictive intelligence including Attendance Trends, Office Occupancy, and Peak Login Arrival Windows.
