# Enterprise Production Scaling & Security Roadmap

This document outlines the architecture, database configurations, and infrastructure requirements to scale the Ramboll Smart Attendance System to **19,000+ users**.

---

## 1. Hosting & Infrastructure Architecture
To support 19,000 active employees logging time continuously, the local development setup must scale to a highly available, load-balanced deployment:

```
                  ┌──────────────────────┐
                  │ 19,000 User Laptops  │
                  └──────────┬───────────┘
                             │ (HTTPS)
                             ▼
                 ┌───────────────────────┐
                 │  Azure Load Balancer  │
                 └──────────┬────────────┘
                            │
               ┌────────────┴────────────┐
               ▼                         ▼
      ┌─────────────────┐       ┌─────────────────┐
      │   API Instance  │       │   API Instance  │
      │ (IIS / AppSvc)  │       │ (IIS / AppSvc)  │
      └────────┬────────┘       └────────┬────────┘
               │                         │
               └────────────┬────────────┘
                            │ (ADO.NET Connection Pool)
                            ▼
               ┌─────────────────────────┐
               │  SQL Server Enterprise  │
               │   (Always On Cluster)   │
               └─────────────────────────┘
```

### Server Specifications
1. **Application Servers**: Two Load-Balanced instances of IIS (Internal Windows Server VMs) or Azure App Service.
   * **Capacity Estimation**: 19,000 users pushing heartbeats every 30 seconds equals **~633 requests per second (RPS)**. Under load-balancing, each server handles ~316 RPS, which requires a minimum of 4 vCPUs and 8GB RAM per server.
2. **Database Cluster**: SQL Server Enterprise Edition configured in an **Always On Availability Group** (Primary Write node + Secondary Read-Only replica for analytics/reports queries).

---

## 2. Database Sync & Master HR Data Feed
For 19,000 employees, manually importing records via SQL insert scripts is impossible. The master list must sync automatically.

```
 [HR System (Workday/SAP)] ──(Daily Sync)──> [Active Directory] ──(Sync)──> [Employee_Master]
```

### Automation Architecture
1. **Azure Data Factory or SQL Agent Jobs**:
   * A scheduled job runs nightly at 02:00, fetching employee updates (new hires, terminations, transfers) from the HR system of record (e.g. Workday).
   * It maps the fields to `Employee_Master` and sets `Is_Active = 0` for terminated employees, immediately disabling their access.
2. **Index Optimization**:
   * Create clustered indexes on `Telemetry_Log(Timestamp)` and partition the table by month to prevent query performance degradation over time (19,000 users will generate ~5 million logs per week).

---

## 3. Microsoft Entra ID (Azure AD) SSO Configuration
To establish Single Sign-On, the application must be registered as an Enterprise Application in Microsoft Entra ID.

```
1. Access Portal ──> 2. Redirect to Entra ID ──> 3. Silent Windows Login ──> 4. JWT Token Returned
```

### App Registration Checklist
Your corporate Azure AD Administrator must configure:
1. **Authentication Type**: Single-page application (SPA) for the Frontend, Web API for the Backend.
2. **Supported Account Types**: Single Tenant (Ramboll Organization Only).
3. **Redirect URIs**:
   * Localhost (Dev): `http://localhost:5000`
   * Production: `https://attendance.ramboll.com`
4. **Token Configuration**: Include the `upn` (User Principal Name, e.g. `employee@ramboll.com`) and `groups` claims in the token.
5. **API Permissions**: Add `User.Read` (Microsoft Graph API).

---

## 4. Cisco ISE Webhook Integration (Alternative to Local Agent)
Rather than deploying a desktop agent app to 19,000 laptops, we can configure **Cisco ISE** to trigger webhooks.

### Process Flow
1. **User Connection**: The employee walks into the office and their laptop automatically connects to the `Ramboll_Corporate` Wi-Fi (802.1X enterprise protocol).
2. **RADIUS Authentication**: Cisco ISE authenticates the MAC Address and Active Directory user.
3. **Webhook Dispatch**: Cisco ISE is configured to send a Syslog Webhook to our API:
   * **URL**: `https://api-attendance.ramboll.com/api/telemetry/ise-log`
   * **Payload**:
     ```json
     {
       "mac": "2C-7B-A0-68-4B-10",
       "user": "bharath@ramboll.com",
       "ip": "10.128.5.42",
       "action": "AUTHENTICATED"
     }
     ```
4. **API Logging**: Our API logs the event, updates the summary, and marks the employee as present.
5. **No Software Needed**: This avoids the need to package, deploy, and maintain desktop agent software on 19,000 machines, saving hundreds of hours of IT support.
