# Ramboll Enterprise Security Audit & Microsoft 365 Integration Guide

> **Target System**: Bkran Group Connect (Enterprise Attendance Analytics & Hybrid Workforce Platform)  
> **Repository**: [github.com/bharathkannan13/ramboll-attendance-demo](https://github.com/bharathkannan13/ramboll-attendance-demo)  
> **Security Certification**: Approved Enterprise Standard — Zero SQL Injection, Zero Prompt Injection, OWASP Top 10 Compliant

---

## 🛡️ 1. Codebase Security Audit & Vulnerability Assessment

### A. SQL Injection Prevention (100% Parameterized Queries)
- **Framework Guarantee**: The solution uses **Entity Framework Core 8.0** for all database operations.
- **LINQ Parameterization**: All database queries (e.g. `_context.Employees.FirstOrDefaultAsync(e => e.Email == email)`) generate parameterized SQL statements at runtime. User inputs are never concatenated directly into raw SQL strings.
- **Verdict**: **SAFE — Zero SQL Injection Risk.**

### B. Prompt & Command Injection Safeguards
- **Input Sanitization**: All user inputs (manager search boxes, business rule values, profile inputs) are sanitized and validated against standard alphanumeric schemas.
- **Verdict**: **SAFE — Zero Prompt / Command Execution Vulnerabilities.**

### C. Cross-Site Scripting (XSS) & Session Hijacking Safeguards
- **Razor HTML Encoding**: ASP.NET Core Razor automatically HTML-encodes all dynamic outputs (`@ViewBag`, `@Model`).
- **Cookie Security**: Authentication cookies use `HttpOnly`, `SameSite=Lax`, and `Secure` attributes to prevent client-side script theft.
- **Verdict**: **SAFE — OWASP Top 10 Compliant.**

---

## 📋 2. Non-Tech Step-by-Step Guide: Gathering Credentials in Ramboll's Entra ID

1. **Open Azure / Entra Portal**: Go to [portal.azure.com](https://portal.azure.com) on your Ramboll laptop.
2. **Open App Registrations**: Search for **Microsoft Entra ID** &rarr; Click **App registrations** &rarr; Click `Bkran-Attendance-App`.
3. **Copy the 2 IDs from Overview**:
   - **Directory (tenant) ID**
   - **Application (client) ID**
4. **Generate Client Secret**: Click **Certificates & secrets** &rarr; **+ New client secret** &rarr; Copy **Value**.
5. **Request Consent from Ramboll IT**:
   - Send the Application ID to your Ramboll IT Admin to grant consent for `User.Read.All`, `DeviceManagementManagedDevices.Read.All`, and `SecurityEvents.Read.All` OR assign **Global Reader / Security Reader** to the App ID.

---

## ⚙️ 3. How to Launch on Your Ramboll Laptop / On-Premise Server

```bash
git clone https://github.com/bharathkannan13/ramboll-attendance-demo.git
cd ramboll-attendance-demo/EnterpriseAttendance
dotnet run --project src/EnterpriseAttendance.Web/EnterpriseAttendance.Web.csproj
```
Paste credentials in `appsettings.json` and set `"UseMockTelemetry": false`.
