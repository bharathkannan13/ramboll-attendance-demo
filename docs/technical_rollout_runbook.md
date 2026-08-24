# Microsoft 365 Ecosystem Integration Runbook & Technical Requirements Specification

> **Document ID**: RUNBOOK-2026-M365-001  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics & Hybrid Workforce Platform)  
> **Target Region**: India Regional Offices (**Chennai, Noida, Hyderabad, Gurugram, Bangalore, Mumbai, Pune, Delhi**)  
> **Data Privacy Policy**: **STRICT INDIA REGION ONLY** (Global & Denmark Accounts Excluded)

---

## 1. Executive Summary & Privacy Protection Mandate

To ensure complete compliance with corporate data privacy standards and European/Global GDPR regulations, **Bkran Group Connect** enforces a strict **Data Localization Filter**. 

### 🔒 Data Privacy & Regional Scope Rules
- **Allowed Scope**: Employees assigned to Indian corporate locations (`officeLocation` matching **Chennai, Noida, Hyderabad, Gurugram, Bangalore, Mumbai, Pune, Delhi**).
- **Hard Exclusion Filter**: All non-India accounts (e.g. Denmark headquarters, European hubs, Americas) are **filtered out at the Microsoft Graph API query level** before any attendance processing or logging occurs.

---

## 2. Microsoft Ecosystem Technical Requirements Checklist

Below are the exact technical credentials, Graph API permissions, and service configurations required from your Microsoft 365 administrator:

### A. Microsoft Entra ID (Formerly Azure AD)
| Requirement | Description | How to Obtain / Configure |
|---|---|---|
| **Directory (Tenant) ID** | GUID identifying your Azure M365 tenant | Azure Portal &rarr; Entra ID &rarr; Overview |
| **Application (Client) ID** | GUID of registered App Registration | Azure Portal &rarr; App Registrations &rarr; Bkran-Attendance-App |
| **Client Secret Value** | Secret key for App-Only OAuth 2.0 auth | Azure Portal &rarr; Certificates & Secrets &rarr; New Client Secret |
| **Graph API Permission** | `User.Read.All` (Application Type) | Azure Portal &rarr; API Permissions &rarr; Add Permission &rarr; Grant Admin Consent |
| **Graph API Permission** | `Directory.Read.All` (Application Type) | Azure Portal &rarr; API Permissions &rarr; Add Permission &rarr; Grant Admin Consent |

### B. Microsoft Intune (Device Management)
| Requirement | Description | How to Obtain / Configure |
|---|---|---|
| **Graph API Permission** | `DeviceManagementManagedDevices.Read.All` | Grants read access to managed laptop inventory, serial numbers, and OS versions |
| **Compliance State Filter** | `complianceState eq 'compliant'` | Only laptops marked **Compliant** generate valid attendance telemetry |

### C. Microsoft Defender for Endpoint (Network Telemetry)
| Requirement | Description | How to Obtain / Configure |
|---|---|---|
| **Graph API Permission** | `SecurityEvents.Read.All` | Ingests device network heartbeats, connected SSID names, and IP address bindings |

---

## 3. Per-State Indian Office Network Infrastructure Matrix

| # | Office Hub Name | State / Region | Corporate Wi-Fi SSIDs | Corporate LAN Subnet CIDRs | Corporate VPN Subnet CIDRs |
|---|---|---|---|---|---|
| 1 | **Chennai Campus** | Tamil Nadu | `Ramboll-CHN-Corp`, `Ramboll-Guest` | `10.100.0.0/16`, `172.16.10.0/24` | `10.200.10.0/24` *(Remote)* |
| 2 | **Bangalore Hub** | Karnataka | `Ramboll-BLR-Corp` | `10.104.0.0/16`, `172.16.40.0/24` | `10.200.40.0/24` *(Remote)* |
| 3 | **Mumbai Tower** | Maharashtra | `Ramboll-MUM-Corp` | `10.105.0.0/16`, `172.16.50.0/24` | `10.200.50.0/24` *(Remote)* |
| 4 | **Pune Tech Hub** | Maharashtra | `Ramboll-PUN-Corp` | `10.106.0.0/16`, `172.16.60.0/24` | `10.200.60.0/24` *(Remote)* |
| 5 | **Delhi Office** | National Capital | `Ramboll-DEL-Corp` | `10.107.0.0/16`, `172.16.70.0/24` | `10.200.70.0/24` *(Remote)* |
| 6 | **Noida Tech Park** | Uttar Pradesh | `Ramboll-NOI-Corp` | `10.101.0.0/16`, `172.16.20.0/24` | `10.200.20.0/24` *(Remote)* |
| 7 | **Hyderabad Hub** | Telangana | `Ramboll-HYD-Corp` | `10.102.0.0/16`, `172.16.30.0/24` | `10.200.30.0/24` *(Remote)* |
| 8 | **Gurugram CyberCity** | Haryana | `Ramboll-GUR-Corp` | `10.103.0.0/16`, `172.16.80.0/24` | `10.200.80.0/24` *(Remote)* |

---

## 4. Multi-Device Correlation & Telemetry Edge Case Logic

### Multi-Device Correlation (Laptop 1 + Laptop 2 -> 1 User Profile)
- When an employee operates **Laptop A** (09:15 AM to 12:30 PM) and **Laptop B** (01:00 PM to 06:15 PM):
  - `DeviceMaster` maps both laptops to `Employee_ID`.
  - The attendance engine merges intervals to yield **First Seen**: `09:15 AM` | **Last Seen**: `06:15 PM` | **Total Office Hours**: `8.50 Hours` without double counting.

---

## 5. Step-by-Step Production Rollout Guide

Set environment variables on Vercel / server:
```env
AzureAd__TenantId="your-azure-tenant-id-guid"
AzureAd__ClientId="your-app-registration-client-id-guid"
AzureAd__ClientSecret="your-client-secret-value"
TelemetrySettings__UseMockTelemetry=false
Smtp__Host="smtp.gmail.com"
Smtp__Port="587"
Smtp__Username="bharathkannan1154@gmail.com"
Smtp__Password="your-16-char-app-password"
Smtp__FromEmail="noreply@bkrangroup.com"
Smtp__TestRecipientEmail="bharathkannan1154@gmail.com"
```
