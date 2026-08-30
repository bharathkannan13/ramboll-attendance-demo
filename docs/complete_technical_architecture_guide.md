# Bkran Group Connect — Complete Technical Architecture & Data Flow Guide

> **Document ID**: ARCH-2026-COMPLETE-001  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics & Hybrid Workforce Platform)  
> **Target Scope**: India Regional Offices (**Chennai, Noida, Hyderabad, Gurugram, Bangalore, Mumbai, Pune, Delhi**)

---

## 🏛️ PART 1: Technical Structure & Clean Architecture Layers

The solution is architected according to **Clean Architecture** principles in **.NET 8.0**:

- `EnterpriseAttendance.Core`: Domain Entities, 18 Enterprise Tables, Enums, Interfaces, DTOs.
- `EnterpriseAttendance.Infrastructure`: EF Core 8.0 `AttendanceDbContext`, Repositories, Microsoft Graph API Client (`Azure.Identity`), Database Seeder.
- `EnterpriseAttendance.Services`: `AttendanceEngine`, `NetworkClassifier` (Bitwise CIDR Math), `OrgHierarchyService` (Recursive Subtree CTEs), `NotificationServices` (Email Narratives & Excel Attachments).
- `EnterpriseAttendance.Web`: ASP.NET Core MVC Controllers, Microsoft Ecosystem Standard Razor Views, Background Services.
- `EnterpriseAttendance.Tests`: xUnit Automated Test Suite.

---

## ⚡ PART 2: Flow of Programming Languages & Execution Pipeline

- **Backend**: C# 12 / .NET 8.0 LTS
- **Frontend**: Vanilla JavaScript (ES6+), Vanilla CSS3 (Microsoft Fluent Ecosystem Standard), Chart.js 4.4
- **ORM & Data**: Entity Framework Core 8.0 (100% Parameterized LINQ)
- **Integration**: Microsoft Graph SDK (`User.Read.All`, `DeviceManagementManagedDevices.Read.All`, `SecurityEvents.Read.All`)

---

## 🗄️ PART 3: 18-Table Enterprise Database Schema

- Core: `Employees`, `Departments`, `OfficeLocations`, `Devices`, `TelemetryEvents`, `AttendanceSessions`, `DailyAttendances`
- Governance & RBAC: `ApplicationOwnership`, `RoleMaster`, `UserRoleEntity`, `PermissionMaster`, `RolePermission`
- Security & Audit: `SecurityAuditLogs`, `LoginSessionLogs`, `ErrorLogs`, `AttendanceRiskLogs`, `AnalyticsLogs`
- System Config: `RetentionConfigs`, `BackupLogs`, `ApiAccessLogs`

---

## 🖥️ PART 4: What We Do in the Portal (Core Capabilities)

1. **Executive Admin Center (`/Admin`)**: Policy configurator, network subnet manager, API log viewer, 18-table REST endpoints.
2. **People Manager Console (`/Manager`)**: Interactive Org Chart Tree, Mon-Fri attendance density chart, direct vs subtree switch, A4 PDF report generator, Excel spreadsheet export, email dispatch to `bharathkannan1154@gmail.com`.
3. **Data Privacy Filter**: Server-side `officeLocation in India Hubs` filter excluding 100% of global/Denmark accounts.
4. **Multi-Device Engine**: Bitwise CIDR matching & multi-laptop merging into single employee timeline.
