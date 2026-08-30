# Document 3: Microsoft Automated Email Notification System Guide

> **Document ID**: DOC-03-EMAIL-AUTOMATION-2026  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Scope**: Microsoft 365 Licensed Mail Services & Automated Manager Dispatches  
> **Primary Recipient**: Engineering Managers (e.g., `bharathkannan1154@gmail.com`)

---

## 📧 1. Architecture of Microsoft Ecosystem Automated Mail

By leveraging Ramboll's licensed **Microsoft 365 Ecosystem**, the solution uses **Microsoft Graph API `Mail.Send`** and **Hosted Background Services** to dispatch automated weekly attendance summaries directly to managers' inboxes.

---

## ⚙️ 2. Step-by-Step Configuration of Automated Manager Email Dispatches

### Licensed Microsoft 365 / SMTP Setup (`appsettings.json`):

```json
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "Username": "bharathkannan1154@gmail.com",
    "Password": "YOUR_16_CHAR_GMAIL_APP_PASSWORD",
    "FromEmail": "noreply@bkrangroup.com",
    "TestRecipientEmail": "bharathkannan1154@gmail.com"
  }
```

- **Schedule**: Every Monday morning at **09:00 AM IST** (`WeeklyManagerEmailBackgroundService`).
- **Target Recipient**: `bharathkannan1154@gmail.com`.
- **Attached Spreadsheet**: `Weekly_Attendance_Report.xlsx`.
- **Single-Click Link**: Redirects to `https://ramboll-attendance-portal.azurewebsites.net/Manager`.
