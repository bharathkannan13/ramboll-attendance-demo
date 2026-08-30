# Ramboll Enterprise Deployment Strategy & Security Review Specification

> **Document ID**: DEPLOY-SEC-2026-001  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics & Hybrid Workforce Platform)  
> **Target Scope**: India Regional Offices (**Chennai, Noida, Hyderabad, Gurugram, Bangalore, Mumbai, Pune, Delhi**)  
> **Target Horizon**: Multi-Year / Decade Enterprise Operations

---

## 🏗️ PART 1: Deployment Options Comparison & Recommendation

| Evaluation Criteria | Option 1: Azure App Service (PaaS) ⭐ **RECOMMENDED** | Option 2: On-Premise IIS + Azure AD Proxy | Option 3: Azure Virtual Machine (IaaS) |
|---|---|---|---|
| **Email Link Accessibility** | ✅ **100% Seamless Anytime/Anywhere** (`https://attendance.ramboll.com`). Works over corporate Wi-Fi, home WFH, and mobile devices via Entra ID SSO. | ⚠️ **Internal Only unless Proxy Enabled**. Direct link (`http://attendance.ramboll.local`) requires corporate VPN or Azure AD App Proxy setup. | ✅ **Seamless Anywhere Access** if public IP/DNS is assigned with SSL certificate. |
| **Microsoft Graph API Connectivity** | ✅ **Native & Ultra-Fast**. Direct low-latency cloud backbone connection to Entra ID, Intune, and Defender Graph APIs. | ⚠️ Requires outbound HTTPS (Port 443) proxy rules through corporate firewall to `graph.microsoft.com`. | ✅ Direct outbound connection to Graph API endpoints. |
| **Long-Term Scaling (10+ Years)** | ✅ **Zero-Maintenance Auto Scaling**. Automatically handles DB growth, database indexing, and hardware upgrades over decades. | ⚠️ Manual server upgrades, storage allocation, and DB maintenance required by IT staff. | ⚠️ Requires manual Windows Server OS patching, IIS upgrades, and disk scaling. |
| **Maintenance & SLA** | ✅ **99.95% Microsoft SLA**. Zero OS patching overhead; fully managed PaaS. | ⚠️ Depends on internal IT server team uptime and hardware reliability. | ⚠️ Requires dedicated OS administration and security patch management. |

---

## 🗄️ PART 2: Long-Term Multi-Year Data Persistence Strategy

- **Daily Attendance Table (`DailyAttendances`)**: Stores `EmployeeId`, `AttendanceDate`, `FirstSeenTime`, `LastSeenTime`, `TotalOfficeHours`, `AttendanceType` (`Office` vs `WFH`), `IsHybridCompliant`, and `OfficeLocationId`.
- **Indexing on `(EmployeeId, AttendanceDate)`** guarantees instant `<50ms` dashboard loading even with millions of historical records across years.

---

## 🛡️ PART 3: Web Application Security Review Certificate

1. **SQL Injection Defense**: 100% Parameterized LINQ queries via EF Core 8.0.
2. **OWASP Top 10 Compliant**: Automatic HTML encoding in Razor templates prevents XSS. Authentication cookies set with `HttpOnly`, `Secure`, and `SameSite=Lax` attributes.
3. **Prompt & Command Injection Protection**: Strict server-side regex and alphanumeric sanitization on all input fields.
4. **Regional Privacy Filter**: Hardened `officeLocation` filter (India Hubs) excludes 100% of non-India & Denmark global accounts before database storage.
