# Document 7: Zero-Code Copy-Paste Configuration & File Location Guide

> **Document ID**: GUIDE-2026-ZERO-CODE-CONFIG-001  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Scenario**: Official Laptop Execution Without AI Prompting / Antigravity Tools  
> **Golden Rule**: **ONLY 1 SINGLE FILE TO EDIT IN THE ENTIRE PROJECT!**

---

## 📁 1. The Exact File Location to Open

📁 **Exact File Path**:  
`src/EnterpriseAttendance.Web/appsettings.json`

---

## 📋 2. The Exact Copy-Paste JSON Template

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

## ☁️ 3. Alternative Method: Azure Portal (No Local Code Editing Required!)

In Azure Portal (`portal.azure.com`) &rarr; App Service (`ramboll-attendance-portal`) &rarr; *Environment variables*, add:
- `TelemetrySettings__UseMockTelemetry` = `false`
- `AzureAd__TenantId` = `[YOUR_TENANT_ID]`
- `AzureAd__ClientId` = `[YOUR_CLIENT_ID]`
- `AzureAd__ClientSecret` = `[YOUR_CLIENT_SECRET]`
- `ConnectionStrings__DefaultConnection` = `[YOUR_AZURE_SQL_CONNECTION_STRING]`
