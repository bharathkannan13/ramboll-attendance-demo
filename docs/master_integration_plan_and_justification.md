# Ramboll Master Integration Plan & Executive Justification Guide

> **Document ID**: MASTER-PLAN-2026-001  
> **Target Audience**: Management, IT Security Heads, Network Administrators & Non-Tech Operations Teams  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics & Hybrid Workforce Platform)  
> **Repository**: [github.com/bharathkannan13/ramboll-attendance-demo](https://github.com/bharathkannan13/ramboll-attendance-demo)

---

## 💡 PART 1: Technical Concepts & Clarifications (Tenant ID vs. Client ID)

1. **Tenant ID**: The unique digital ID of the entire Ramboll Microsoft 365 cloud environment. Every employee and laptop in Ramboll shares the **exact same Tenant ID**.
2. **Client ID**: The unique identifier for the **Bkran Attendance Portal application**.
3. **Are Client IDs different for each laptop?**: **NO! IT IS THE EXACT SAME SINGLE CLIENT ID FOR ALL EMPLOYEES!** One single App Registration in Azure handles telemetry for all 10,000+ laptops across India.

---

## 💼 PART 2: Executive Justification — "Why Do We Need Microsoft Graph API?"

1. **Zero-Cost Badgeless Automation**: Eliminates physical turnstile badge swipes by correlating Wi-Fi/LAN connection heartbeats via Defender Graph API.
2. **Strict Privacy & Data Localization**: Server-side filters (`officeLocation in India Hubs`) exclude non-India & Denmark accounts before any data is logged.
3. **Managed Laptop Security Enforcement**: Queries Intune Graph API to verify laptop compliance state.
4. **Accurate First Seen / Last Seen Timestamps**: Automatically records network arrival (First Seen) and departure (Last Seen).

---

## 🌐 PART 3: Non-Tech On-Premise Server & Internal Domain Setup

To create an internal domain like `http://attendance.ramboll.local`:
1. **Publish App**: `dotnet publish src/EnterpriseAttendance.Web/EnterpriseAttendance.Web.csproj -c Release -o C:\inetpub\wwwroot\BkranAttendance`
2. **Bind IIS Web Site**: Bind hostname `attendance.ramboll.local` to site folder `C:\inetpub\wwwroot\BkranAttendance`.
3. **DNS Mapping**: Add internal DNS A-Record mapping `attendance.ramboll.local` to Server IP (`10.100.5.20`).
