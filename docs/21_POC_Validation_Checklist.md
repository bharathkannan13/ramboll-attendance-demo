# Enterprise Attendance & Workforce Analytics Platform

## POC Validation Checklist

### 1. Executive Summary

This document serves as the formal validation checklist for the Proof of Concept (POC) phase of the Enterprise Attendance platform. The POC is designed to mitigate the highest technical risks before full-scale implementation. 

### 2. POC Objectives

The primary objective of this POC is to **prove that employee attendance can be accurately and reliably calculated using passive Microsoft 365 telemetry (Intune, Defender) and corporate network configurations, without requiring the installation of any endpoint agents.**

Specifically, the POC must prove:
1. We can ingest network telemetry.
2. We can differentiate between Corporate Office LAN/Wi-Fi and Home/VPN networks.
3. We can merge telemetry events into coherent daily attendance sessions.

---

### 3. Validation Categories

*Instructions for QA/Testers: Execute each scenario. Fill in the "Actual Result", mark the Status (Pass/Fail/NA), and add Notes for any anomalies.*

#### Category 1: Microsoft Integration Validation

| ID | Checklist Item | Expected Result | Actual Result | Status | Notes |
|---|---|---|---|---|---|
| M1 | Entra ID user sync works | Users from test tenant sync to local DB. | | | |
| M2 | Org chart hierarchy built | Multi-level manager relationships establish correctly. | | | |
| M3 | India office filter applied | Only users mapped to IN offices sync. | | | |
| M4 | Intune device sync works | Devices associated with synced users are imported. | | | |
| M5 | Compliance status retrieved | Device compliance (Yes/No) is logged. | | | |
| M6 | Defender telemetry ingested | Network events pull successfully via API. | | | |
| M7 | Network adapter info captured | Payloads contain SSID, Subnet, and Gateway MAC. | | | |
| M8 | Token management | MSAL acquires and caches tokens successfully. | | | |
| M9 | Retry on transient failures | API calls retry on 429/503 errors (Polly). | | | |
| M10 | Mock provider functionality | Mock telemetry generator works without Azure keys. | | | |

#### Category 2: Network Classification Validation

| ID | Checklist Item | Expected Result | Actual Result | Status | Notes |
|---|---|---|---|---|---|
| N1 | Corp SSID classified Office | Telemetry with known SSID marks device as Office. | | | |
| N2 | Corp Subnet classified Office | Telemetry with known IP range marks as Office. | | | |
| N3 | Home Wi-Fi classified Remote | Unknown SSID marks as Remote. | | | |
| N4 | VPN classified Remote (CRITICAL)| Known VPN subnets explicitly mark as Remote. | | | |
| N5 | Unknown network classified | Completely foreign network logs as Unknown. | | | |
| N6 | Per-office matching | Telemetry matching Chennai config maps to Chennai. | | | |
| N7 | Network config CRUD | Admin can add/edit/delete SSIDs and Subnets. | | | |
| N8 | Classification audit logging | System logs *why* a decision was made (e.g., "Matched SSID X"). | | | |
| N9 | Conflict resolution | Device reporting both Corp IP and Home SSID resolves safely. | | | |
| N10 | Case insensitivity | SSID matching ignores case (`Ramboll_Corp` == `ramboll_corp`).| | | |

#### Category 3: Attendance Engine Validation

| ID | Checklist Item | Expected Result | Actual Result | Status | Notes |
|---|---|---|---|---|---|
| A1 | Session creation | First Office telemetry creates a new "Active" session. | | | |
| A2 | Last Seen updates | Subsequent telemetry updates the session's Last Seen time. | | | |
| A3 | Session closure triggers | Disconnect/Sleep telemetry closes the session. | | | |
| A4 | Grace period merges gaps | 15-minute gap merges two sessions into one continuous block. | | | |
| A5 | Grace period exceeded | 60-minute gap creates two distinct sessions. | | | |
| A6 | End-of-day merge | Daily background job calculates total hours correctly. | | | |
| A7 | Multi-device merge | Laptop and Mobile in office merge into one user session. | | | |
| A8 | Non-compliant device | Telemetry from non-compliant/unmanaged devices is ignored. | | | |
| A9 | Duplicate filtering | Identical telemetry payloads within 1 min are dropped. | | | |
| A10 | First/Last Seen accuracy | Start time and End time strictly match telemetry bounds. | | | |
| A11 | Office hours vs Breaks | Lunch break > grace period is deducted from total hours. | | | |
| A12 | Remote hours exclusion | Time spent on Home Wi-Fi is 0 Office Hours. | | | |
| A13 | Weekly count | 3 days in office correctly counts as 3/3 for the week. | | | |
| A14 | Monthly summary | Total days and hours roll up accurately per month. | | | |
| A15 | Confidence scoring | Session confidence calculated based on telemetry density. | | | |

#### Category 4: Dashboard Validation

| ID | Checklist Item | Expected Result | Actual Result | Status | Notes |
|---|---|---|---|---|---|
| D1 | Employee View loads | User sees own daily/weekly attendance. | | | |
| D2 | Manager View loads | Manager sees their direct reports' status. | | | |
| D3 | Drill-down works | Clicking a day shows session details. | | | |
| D4 | Discrepancy submission | User can flag a day as incorrect. | | | |
| D5 | Admin Network UI | Admins can view and edit network rules. | | | |
| D6 | Export functionality | Data exports to CSV successfully. | | | |
| D7 | Pagination works | Large tables paginate correctly (server-side). | | | |
| D8 | Mobile responsiveness | UI is usable on mobile browsers. | | | |
| D9 | API performance | Endpoints return data < 500ms. | | | |
| D10 | UI Error handling | API failures show graceful error toasts. | | | |

#### Category 5: RBAC Validation

| ID | Checklist Item | Expected Result | Actual Result | Status | Notes |
|---|---|---|---|---|---|
| R1 | Employee access | Can only see own data. Cannot see peers. | | | |
| R2 | Manager access | Can see own data and direct reports. | | | |
| R3 | Skip-level manager | Can see direct reports AND their reports. | | | |
| R4 | HR access | Can see all users, read-only. | | | |
| R5 | Admin access | Can edit configurations. | | | |
| R6 | Unauthorized API | API returns 403 when Manager tries Admin route. | | | |
| R7 | Token claims | JWT contains correct role claims. | | | |
| R8 | UI elements hidden | Admin buttons hidden from Employees. | | | |

#### Category 6: Email Notification Validation

| ID | Checklist Item | Expected Result | Actual Result | Status | Notes |
|---|---|---|---|---|---|
| E1 | Weekly summary | Email sent to user with weekly stats. | | | |
| E2 | Non-compliance alert | Email sent to manager if employee < 3 days. | | | |
| E3 | Discrepancy filed | Email sent to manager for approval. | | | |
| E4 | Discrepancy resolved | Email sent to employee when resolved. | | | |
| E5 | SMTP configuration | System connects to SMTP server successfully. | | | |

#### Category 7: Security Validation

| ID | Checklist Item | Expected Result | Actual Result | Status | Notes |
|---|---|---|---|---|---|
| S1 | No agent installed | Validation that no software was pushed to test laptops. | | | |
| S2 | No plaintext secrets | Key Vault used; no secrets in appsettings.json. | | | |
| S3 | TLS enforced | All API traffic is HTTPS only. | | | |
| S4 | SQL Injection prevented | EF Core parameterization verified. | | | |
| S5 | XSS prevented | UI properly encodes user inputs. | | | |
| S6 | PII protected | No exact home IP addresses logged in plaintext. | | | |
| S7 | CORS configured | Only allowed origins can call the API. | | | |
| S8 | Token expiration | JWTs expire and require refresh. | | | |

#### Category 8: Performance Validation

| ID | Checklist Item | Expected Result | Actual Result | Status | Notes |
|---|---|---|---|---|---|
| P1 | Telemetry ingestion rate | Can process 1000 events/sec. | | | |
| P2 | Job execution time | End-of-day job for 100 users finishes < 1 min. | | | |
| P3 | DB indexing | Query plans show index usage for telemetry searches. | | | |
| P4 | Memory profile | App Service stays within memory bounds during load. | | | |
| P5 | Concurrency | Handles concurrent manager logins seamlessly. | | | |

---

### 4. POC Demo Script

To demonstrate the POC to stakeholders, follow these steps:
1. **Setup**: Show the empty database and Admin dashboard.
2. **Network Config**: Add a mock "Ramboll Noida" network config (SSID: `Ramboll_Corp_Noida`).
3. **Telemetry Injection (Office)**: Run the mock provider to simulate "Employee A" connecting to `Ramboll_Corp_Noida`. 
4. **Verification 1**: Refresh the Manager Dashboard. Show "Employee A" is actively "In Office".
5. **Telemetry Injection (Home/VPN)**: Simulate "Employee B" connecting via VPN subnet.
6. **Verification 2**: Show "Employee B" is registered as "Remote".
7. **End of Day**: Manually trigger the EOD Quartz job.
8. **Verification 3**: Show the calculated hours for Employee A and 0 hours for Employee B.

### 5. Known Limitations
- The POC utilizes a mock telemetry provider for high-volume load testing to avoid hitting Microsoft Graph API rate limits on the test tenant.
- Production Graph API integration requires specific Enterprise Admin consents which are mocked in this environment.

### 6. Sign-off Template

POC execution has been reviewed and approved. The technical approach is validated.

| Name | Role | Signature | Date |
|---|---|---|---|
| | QA Lead | | |
| | Lead Architect | | |
| | Project Sponsor | | |
