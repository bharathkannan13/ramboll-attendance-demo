# Ramboll On-Premise Server Deployment & State Network Infrastructure Guide

> **Document ID**: DEPLOY-2026-ONPREM-001  
> **System Name**: Bkran Group Connect  
> **Target Environment**: On-Premise Corporate Server / Windows Server 2022 / IIS / Kestrel  
> **Target Scope**: India Regional Offices (**Chennai, Bangalore, Mumbai, Pune, Delhi, Noida, Hyderabad, Gurugram**)

---

## 1. Detailed Azure Credentials Extraction & Permission Assignment

### A. How to Get the 3 Required Credentials in Entra ID Portal

1. **Directory (Tenant) ID**: Open [entra.microsoft.com](https://entra.microsoft.com) &rarr; Copy Tenant ID.
2. **Application (Client) ID**: Go to **App registrations** &rarr; Select `Bkran-Attendance-App` &rarr; Copy Application ID.
3. **Client Secret Value**: Go to **Certificates & secrets** &rarr; **New client secret** &rarr; Copy Secret **Value**.
4. **Global / Security Reader Consent**:
   - Request Ramboll Azure Admin to grant Admin Consent for `User.Read.All`, `DeviceManagementManagedDevices.Read.All`, `SecurityEvents.Read.All` OR assign **Global Reader / Security Reader** to the App ID.

---

## 2. On-Premise Corporate Server Deployment Instructions

```powershell
# 1. Publish Release Bundle
dotnet publish src/EnterpriseAttendance.Web/EnterpriseAttendance.Web.csproj -c Release -o C:\inetpub\wwwroot\BkranAttendance

# 2. Configure appsettings.json with Tenant ID, Client ID, Client Secret, and SQL Server Connection String

# 3. Create IIS Site bound to http://0.0.0.0:8080 or http://attendance.ramboll.local
```

---

## 3. State-by-State Network Infrastructure Configuration Matrix

| State / Hub | City | Corporate Wi-Fi SSIDs | Corporate LAN Subnet CIDRs | Public Gateway IPs | VPN Gateway Subnets *(Classified WFH)* |
|---|---|---|---|---|---|
| **Tamil Nadu** | Chennai | `Ramboll-CHN-Corp` | `10.100.0.0/16` | `122.160.10.1` | `10.200.10.0/24` |
| **Karnataka** | Bangalore | `Ramboll-BLR-Corp` | `10.104.0.0/16` | `122.160.40.1` | `10.200.40.0/24` |
| **Maharashtra** | Mumbai | `Ramboll-MUM-Corp` | `10.105.0.0/16` | `122.160.50.1` | `10.200.50.0/24` |
| **Maharashtra** | Pune | `Ramboll-PUN-Corp` | `10.106.0.0/16` | `122.160.60.1` | `10.200.60.0/24` |
| **Delhi NCR** | Delhi | `Ramboll-DEL-Corp` | `10.107.0.0/16` | `122.160.70.1` | `10.200.70.0/24` |
| **Uttar Pradesh** | Noida | `Ramboll-NOI-Corp` | `10.101.0.0/16` | `122.160.20.1` | `10.200.20.0/24` |
| **Telangana** | Hyderabad | `Ramboll-HYD-Corp` | `10.102.0.0/16` | `122.160.30.1` | `10.200.30.0/24` |
| **Haryana** | Gurugram | `Ramboll-GUR-Corp` | `10.103.0.0/16` | `122.160.80.1` | `10.200.80.0/24` |
