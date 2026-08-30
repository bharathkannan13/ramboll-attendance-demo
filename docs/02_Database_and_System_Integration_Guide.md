# Document 2: Database Architecture & System Integration Guide

> **Document ID**: DOC-02-DATABASE-INTEGRATION-2026  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Scope**: Enterprise Relational Database & Microsoft 365 Ecosystem Integrations  
> **Data Privacy Policy**: **STRICT INDIA REGION ONLY** (Global & Denmark Accounts Excluded)

---

## 🗄️ 1. Database Architecture & 18-Table Schema

- **Core**: `Employees`, `Departments`, `OfficeLocations`, `Devices`, `TelemetryEvents`, `AttendanceSessions`, `DailyAttendances`
- **Governance & RBAC**: `ApplicationOwnership`, `RoleMaster`, `UserRoleEntity`, `PermissionMaster`, `RolePermission`
- **Security & Audit**: `SecurityAuditLogs`, `LoginSessionLogs`, `ErrorLogs`, `AttendanceRiskLogs`, `AnalyticsLogs`
- **System Config**: `RetentionConfigs`, `BackupLogs`, `ApiAccessLogs`

---

## 🔒 2. Data Privacy & Regional Filtering Engine

- **Server-Side Filter**: Ingests only users where `officeLocation` matches Indian hubs (**Chennai, Noida, Hyderabad, Gurugram, Bangalore, Mumbai, Pune, Delhi**). Global accounts are filtered out automatically.

---

## 🔑 3. Microsoft Graph API Permissions Matrix

- `User.Read.All`: Syncs employee directory and reporting hierarchy.
- `DeviceManagementManagedDevices.Read.All`: Reads Intune laptop inventory and compliance state.
- `SecurityEvents.Read.All`: Ingests Wi-Fi SSIDs, IP subnets, and network heartbeats.
