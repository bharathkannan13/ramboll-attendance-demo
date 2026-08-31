# Master Pre-Pilot Deployment & Configuration Playbook (1 Manager + 4 Direct Reports)

> **Document ID**: PLAYBOOK-2026-PREPILOT-OPTIMIZED  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Pre-Pilot Scope**: **1 Organization Unit | 1 Manager | 4 Direct Reporting Employees (5 Users Total)**  
> **Primary Pilot Manager**: `bharathkannan1154@gmail.com`

---

## 📌 Executive Summary of Pre-Pilot Testing Scope

- **Test Audience**: 1 Manager + 4 Direct Reporting Staff (5 test laptops total).
- **Goal**: Validate First Seen / Last Seen precision, Office Wi-Fi vs WFH flagging, and automated email dispatches with attached Excel spreadsheets before full corporate rollout.

---

## 💻 PHASE 1: GitHub Transfer & Laptop Zero-Code Setup

Open `src/EnterpriseAttendance.Web/appsettings.json` on your laptop & replace completely with:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",

  "TelemetrySettings": {
    "UseMockTelemetry": false,
    "PrePilotModeOnly": true,
    "PrePilotManagerEmail": "bharathkannan1154@gmail.com",
    "IndiaRegionalFilterOnly": true,
    "SyncIntervalMinutes": 15
  },

  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "[PASTE_YOUR_AZURE_TENANT_ID_HERE]",
    "ClientId": "[PASTE_YOUR_AZURE_CLIENT_ID_HERE]",
    "ClientSecret": "[PASTE_YOUR_CLIENT_SECRET_VALUE_HERE]"
  },

  "MailSettings": {
    "Provider": "MicrosoftGraph",
    "SenderEmail": "attendance-response@ramboll.com",
    "SenderDisplayName": "Bkran Group Connect Attendance System",
    "LiveDomainUrl": "https://ramboll-attendance-portal.azurewebsites.net",
    "EnableWeeklyManagerReport": true,
    "TestRecipientEmail": "bharathkannan1154@gmail.com"
  },

  "ConnectionStrings": {
    "DefaultConnection": "[PASTE_YOUR_AZURE_SQL_CONNECTION_STRING_HERE]"
  }
}
```

---

## 🔑 PHASE 2: Entra ID SSO & App Registration Setup

1. In **[entra.microsoft.com](https://entra.microsoft.com)** &rarr; Register `Bkran-Attendance-Engine-PrePilot`.
2. Copy `Application (client) ID` and `Directory (tenant) ID`.
3. Create Client Secret & copy Secret Value.
4. Under *Authentication*, add Web Redirect URI `https://ramboll-attendance-portal.azurewebsites.net/signin-oidc` & check **ID tokens**.

---

## 🔒 PHASE 3: Microsoft Graph API Permissions Setup

1. Under *API permissions*, add Application Permissions: `User.Read.All`, `DeviceManagementManagedDevices.Read.All`, `SecurityEvents.Read.All`, `Mail.Send`.
2. Click **Grant admin consent for Ramboll**.

---

## 📧 PHASE 4: Pre-Pilot Automated Mail Service Configuration

1. In **[admin.microsoft.com](https://admin.microsoft.com)** &rarr; Create shared mailbox `attendance-response@ramboll.com`.
2. Every Monday 09:00 AM IST, dispatches email to Manager (`bharathkannan1154@gmail.com`) with attached `Weekly_Attendance_Report.xlsx` containing the 4 direct reports and a single-click redirection link to `https://ramboll-attendance-portal.azurewebsites.net/Manager`.

---

## ☁️ PHASE 5: Azure App Service & SQL Cloud Setup

In Azure Portal &rarr; App Service `ramboll-attendance-portal` &rarr; *Environment variables*, add:
- `TelemetrySettings__UseMockTelemetry` = `false`
- `TelemetrySettings__PrePilotModeOnly` = `true`
- `TelemetrySettings__PrePilotManagerEmail` = `bharathkannan1154@gmail.com`
- `AzureAd__TenantId` = `[YOUR_TENANT_ID]`
- `AzureAd__ClientId` = `[YOUR_CLIENT_ID]`
- `AzureAd__ClientSecret` = `[YOUR_CLIENT_SECRET]`
- `ConnectionStrings__DefaultConnection` = `[YOUR_AZURE_SQL_CONNECTION_STRING]`

---

## 🧪 PHASE 6: The 4 Pre-Pilot Checkpoints & Production Promotion

- **Check 1**: First Seen / Last Seen timestamp precision.
- **Check 2**: Office Wi-Fi (`10.100.0.0/16`) vs WFH flagging.
- **Check 3**: Automated Monday email dispatch to Manager (`bharathkannan1154@gmail.com`) with 4-person Excel report.
- **Check 4**: Hyperlink redirection button to `https://ramboll-attendance-portal.azurewebsites.net/Manager`.

**Production Promotion**: Change `TelemetrySettings__PrePilotModeOnly` to `false` in Azure Portal to expand from 5 pre-pilot users to all employees!
