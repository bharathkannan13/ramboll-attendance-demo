# Document 10: Per-Manager Excel Generation & SSO Scope Isolation Guide

> **Document ID**: TECH-2026-EXCEL-SSO-ISOLATION-001  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Scope**: Per-Manager Subtree Querying, Custom Excel Attachment Generation, & Entra ID SSO Scope Isolation

---

## 📌 1. Executive Summary & Security Isolation Principle

1. **Automated Weekly Email**: Each manager receives a **customized `Weekly_Attendance_Report.xlsx`** containing ONLY their direct and indirect reports (subordinates).
2. **Dashboard Hyperlink Access**: When a manager clicks `https://ramboll-attendance-portal.azurewebsites.net/Manager`, Entra ID Single Sign-On (SSO) authenticates their corporate email. The backend queries SQL Server using a **Recursive CTE Tree Query** to display ONLY their team hierarchy.

---

## 💻 2. Technical Implementation: Per-Manager Excel Generation

- **Recursive CTE Query (`OrgHierarchyService.cs`)**: Traverses SQL database N levels deep starting from the logged-in `ManagerId` to return only subordinates.
- **Excel Building (`NotificationServices.cs`)**: Builds `Weekly_Attendance_Report.xlsx` dynamically in memory for each manager containing their team's Mon–Fri presence, First Seen, Last Seen, and hybrid compliance status (`MET` vs `NON-COMPLIANT`).

---

## 🔒 3. Single Sign-On (SSO) & Dashboard Scope Isolation

- **Entra ID Claim Extraction**: `ManagerController.cs` reads the `EmployeeId` claim from the manager's SSO token.
- **Scope Isolation Guarantee**: Manager A cannot see Manager B's subordinates. The recursive CTE strictly scopes data at the database query level.
