# Master Pre-Pilot Deployment Playbook (Dynamic Single Manager Input)

> **Document ID**: PLAYBOOK-2026-DYNAMIC-SINGLE-MANAGER  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Pre-Pilot Horizon**: **Dynamic Single Manager Email Input & Automatic Subordinate Discovery**  
> **Input Rule**: **Provide ONLY 1 Manager Email (e.g., `manager.name@ramboll.com`)**

---

## 📌 Executive Overview: Dynamic Subordinate Discovery

Simply provide **ONE Manager Email** in the configuration (`"PrePilotManagerEmail": "your.manager@ramboll.com"`). The application automatically:
1. Queries Microsoft Graph API (`GET /users/{id}/directReports`) to discover all reporting staff under that manager.
2. Ingests Intune & Defender laptop telemetry exclusively for that manager's team.
3. Generates the customized `Weekly_Attendance_Report.xlsx` for that manager's subordinates.
4. Sends the weekly automated email dispatch to that manager's inbox with the Excel report attached and the live SSO redirection button.
5. Authenticates that manager via Entra ID Single Sign-On (SSO) and displays their exact team Org Chart tree in the Manager Console.

---

## 💻 Zero-Code Copy-Paste Template (`appsettings.json`)

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
    "PrePilotManagerEmail": "[PASTE_ANY_MANAGER_EMAIL_HERE]",
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
    "TestRecipientEmail": "[PASTE_ANY_MANAGER_EMAIL_HERE]"
  },

  "ConnectionStrings": {
    "DefaultConnection": "[PASTE_YOUR_AZURE_SQL_CONNECTION_STRING_HERE]"
  }
}
```

---

## ☁️ Azure Portal Environment Variables (`portal.azure.com`)

In Azure Portal &rarr; App Service `ramboll-attendance-portal` &rarr; *Environment variables*, add:
- `TelemetrySettings__UseMockTelemetry` = `false`
- `TelemetrySettings__PrePilotModeOnly` = `true`
- `TelemetrySettings__PrePilotManagerEmail` = `[PASTE_ANY_MANAGER_EMAIL_HERE]`
- `AzureAd__TenantId` = `[YOUR_AZURE_TENANT_ID]`
- `AzureAd__ClientId` = `[YOUR_CLIENT_ID]`
- `AzureAd__ClientSecret` = `[YOUR_CLIENT_SECRET]`
- `ConnectionStrings__DefaultConnection` = `[YOUR_AZURE_SQL_CONNECTION_STRING]`

---

## 🚀 One-Click Promotion to Full Production

Once you verify the email report and dashboard for your test manager:
1. Open Azure Portal &rarr; App Service `ramboll-attendance-portal` &rarr; *Environment variables*.
2. Change `TelemetrySettings__PrePilotModeOnly` to `false`.
3. Click **Apply** &rarr; **Confirm**.
4. **Done!** The system automatically expands from the single pilot manager to all managers across Ramboll!
