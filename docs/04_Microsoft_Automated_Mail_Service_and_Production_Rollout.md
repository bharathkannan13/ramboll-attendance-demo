# Stage 4 Guide: Microsoft Automated Mail System & Production Rollout

> **Document ID**: STAGE-04-EMAIL-ROLLOUT-2026  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Scope**: Tool Locations, Background Scheduling, Attachment Generation, & Redirection Config

---

## 📌 Executive Summary & Tool Locations

| Component / Feature | Exact Configuration Tool | Configuration Parameter / Code File | What It Does |
|---|---|---|---|
| **1. Sender Account** | Azure Portal & `appsettings.json` | `"MailSettings:SenderEmail": "attendance-response@ramboll.com"` | Specifies the corporate shared mailbox address. |
| **2. Live Domain URL** | Azure Portal & `appsettings.json` | `"MailSettings:LiveDomainUrl": "https://ramboll-attendance-portal.azurewebsites.net"` | Specifies the redirection URL embedded in email buttons. |
| **3. Schedule (Mon 9 AM)** | C# Backend Code | `WeeklyManagerEmailBackgroundService.cs` | Runs a background timer every Monday at 09:00 AM IST automatically. |
| **4. Recipient Filter** | C# Backend Code | `OrgHierarchyService.GetActiveManagersAsync()` | Queries SQL database for all active people managers. |
| **5. Excel Attachment** | C# Backend Code | `NotificationServices.GenerateWeeklyExcelAttachment()` | Generates `Weekly_Attendance_Report.xlsx` in memory. |
| **6. Email Redirection Link** | HTML Template & C# Code | `NotificationServices.cs` | Creates the `<a href="https://.../Manager">` button in email body. |

---

## ⚙️ Step-by-Step Configuration Steps in Each Tool

### STEP A: Configure Settings in `appsettings.json` (or Azure Portal)

```json
{
  "MailSettings": {
    "Provider": "MicrosoftGraph",
    "SenderEmail": "attendance-response@ramboll.com",
    "SenderDisplayName": "Bkran Group Connect Attendance System",
    "LiveDomainUrl": "https://ramboll-attendance-portal.azurewebsites.net",
    "EnableWeeklyManagerReport": true,
    "WeeklyReportDay": "Monday",
    "WeeklyReportTime": "09:00"
  }
}
```

### STEP B: C# Background Service (`WeeklyManagerEmailBackgroundService.cs`)
You **do not need external cron tools**. The C# Hosted Background Service runs continuously inside your Azure App Service, querying managers, generating `Weekly_Attendance_Report.xlsx`, embedding the live domain link, and dispatching via Graph API `Mail.Send` every Monday at 09:00 AM IST.
