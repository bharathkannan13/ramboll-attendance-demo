# Master Enterprise Deployment & Configuration Playbook

> **Document ID**: MASTER-PLAYBOOK-2026-FINAL  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Scope**: End-to-End Click-by-Click Enterprise Setup & Technical Requirements Guide  
> **Target Scope**: Ramboll India Regional Offices (**Chennai, Noida, Hyderabad, Gurugram, Bangalore, Mumbai, Pune, Delhi**)

---

## 📌 SECTION 1: Single Sign-On (SSO) & Azure App Registration

1. Open **[entra.microsoft.com](https://entra.microsoft.com)** &rarr; Applications &rarr; App registrations &rarr; Click **+ New registration**.
2. Name: `Bkran-Attendance-Engine`. Click **Register**.
3. Under *Overview*, copy **Application (client) ID** and **Directory (tenant) ID**.
4. Under *Certificates & secrets*, click **+ New client secret** &rarr; Copy Secret Value.
5. Under *Authentication*, add Web Redirect URI `https://ramboll-attendance-portal.azurewebsites.net/signin-oidc` & check **ID tokens**.

---

## 🔑 SECTION 2: Microsoft Graph API Permissions (Entra, Intune, Defender, Mail)

1. Under *API permissions* &rarr; Click **+ Add a permission** &rarr; Select **Microsoft Graph** &rarr; **Application permissions**.
2. Add: `User.Read.All`, `DeviceManagementManagedDevices.Read.All`, `SecurityEvents.Read.All`, `Mail.Send`.
3. Click **Grant admin consent for Ramboll**.

---

## 📧 SECTION 3: Microsoft 365 Automated Mail Service Setup

1. Open **[admin.microsoft.com](https://admin.microsoft.com)** &rarr; Teams & groups &rarr; Shared mailboxes &rarr; Click **+ Add a shared mailbox**.
2. Name: `Bkran Attendance System` | Email: `attendance-response@ramboll.com`. Click **Save**.

---

## ☁️ SECTION 4: Azure SQL Database & Azure App Service Cloud Deployment

1. **Azure SQL Database**: Provision SQL Database `sqldb-attendance-production` in `portal.azure.com` & copy ADO.NET Connection String.
2. **Azure App Service**: Provision .NET 8.0 Linux/Windows App Service `ramboll-attendance-portal`.
3. **Deployment Center**: Link GitHub repo `bharathkannan13/ramboll-attendance-demo` branch `main`.

---

## ⚙️ SECTION 5: Configuration Key Injection (Environment Variables)

In Azure Portal &rarr; App Service `ramboll-attendance-portal` &rarr; *Environment variables*, add:
- `TelemetrySettings__UseMockTelemetry` = `false`
- `AzureAd__TenantId` = `[YOUR_TENANT_ID]`
- `AzureAd__ClientId` = `[YOUR_CLIENT_ID]`
- `AzureAd__ClientSecret` = `[YOUR_CLIENT_SECRET]`
- `ConnectionStrings__DefaultConnection` = `[YOUR_AZURE_SQL_CONNECTION_STRING]`

---

## 🌐 SECTION 6: India Regional Network Subnet CIDR Matrix

| State / Region | Office Location | Corporate Wi-Fi SSID | Subnet CIDR Range | Classification |
|---|---|---|---|---|
| **Tamil Nadu** | Chennai | `Ramboll-CHN-Corporate` | `10.100.0.0/16` | **OFFICE** (Chennai Hub) |
| **NCR / UP** | Noida | `Ramboll-NOI-Corporate` | `10.101.0.0/16` | **OFFICE** (Noida Hub) |
| **Telangana** | Hyderabad | `Ramboll-HYD-Corporate` | `10.102.0.0/16` | **OFFICE** (Hyderabad Hub) |
| **NCR / Haryana** | Gurugram | `Ramboll-GUG-Corporate` | `10.103.0.0/16` | **OFFICE** (Gurugram Hub) |
| **Karnataka** | Bangalore | `Ramboll-BLR-Corporate` | `10.104.0.0/16` | **OFFICE** (Bangalore Hub) |
| **Maharashtra** | Mumbai | `Ramboll-MUM-Corporate` | `10.105.0.0/16` | **OFFICE** (Mumbai Hub) |
| **Maharashtra** | Pune | `Ramboll-PUN-Corporate` | `10.106.0.0/16` | **OFFICE** (Pune Hub) |
| **Delhi** | Delhi | `Ramboll-DEL-Corporate` | `10.107.0.0/16` | **OFFICE** (Delhi Hub) |
| *All States* | *Home / WFH* | *Home Wi-Fi / Hotspot* | `192.168.x.x` / `10.200.x.x` (VPN) | **REMOTE WFH** |
