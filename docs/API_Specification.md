# REST API Specification

> **Document ID**: API-2026-ENT-004  
> **Base URL**: `https://ramboll-attendance-demo.vercel.app/api`  

---

## 1. Enterprise Core API Endpoints

### Governance & Governance API
- **`GET /api/enterprise/governance`**: Returns application business & technical ownership metadata (`Application_Ownership`).

### RBAC API
- **`GET /api/enterprise/rbac`**: Returns active roles and permission mappings (`Role_Master`).

### Multi-Location Offices API
- **`GET /api/enterprise/offices`**: Returns all 8 Indian corporate offices (**Chennai, Bangalore, Mumbai, Pune, Delhi, Noida, Hyderabad, Gurugram**).

### Work Modes API
- **`GET /api/enterprise/work-modes`**: Returns valid work modes (**Office, WFH, Client Site, Travel**).

### Security & Audit API
- **`GET /api/enterprise/audit-logs`**: Returns security audit trails (`Security_Audit_Log`).
- **`GET /api/enterprise/sessions`**: Returns active user login sessions (`Login_Session_Log`).
- **`GET /api/enterprise/error-logs`**: Returns error logs categorized by severity (`Error_Log`).

### Cybersecurity Risks & AI Analytics API
- **`GET /api/enterprise/cybersecurity-risks`**: Returns flagged cybersecurity risk logs (`Attendance_Risk_Log`).
- **`GET /api/enterprise/ai-analytics`**: Returns AI occupancy and attendance trend predictions (`Analytics_Log`).

---

## 2. Attendance & Reporting API

- **`GET /api/admin/branch-occupancy`**: Per-Office Branch Occupancy Heatmap Data.
- **`GET /api/admin/department-compliance`**: Departmental Compliance Comparison Matrix Data.
- **`GET /api/admin/sync-health`**: M365 Graph API Ecosystem Sync Status.
- **`GET /api/manager/{id}/day-of-week-distribution`**: Monday–Friday Presence Density Heatmap.
- **`GET /api/reports/pdf-report/{id}`**: A4 Printable Executive PDF Summary.
- **`GET /api/reports/weekly-excel/{id}`**: Weekly Team Attendance Excel Spreadsheet (`.xlsx`).
