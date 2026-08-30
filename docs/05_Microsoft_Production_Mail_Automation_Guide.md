# Document 5: Microsoft Production Automated Mail System & Service Account Guide

> **Document ID**: DOC-05-PROD-EMAIL-AUTOMATION-2026  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Scope**: Enterprise Production Email Dispatches via Microsoft 365 Licensing  
> **Corporate Sender Address**: `attendance-response@ramboll.com`

---

## 📌 1. Executive Summary & Recommended Microsoft Service

For pre-production and production rollout across Ramboll's manager workforce, replacing Gmail with **Ramboll's Licensed Microsoft 365 Ecosystem** is the enterprise standard.

### 🏆 Recommended Choice: **Microsoft Graph API `Mail.Send` (Application Permission)**

- **Service Account / Shared Mailbox**: `attendance-response@ramboll.com`
- **Why Best**: Zero passwords needed in code! Uses secure OAuth 2.0 app token. Works directly via HTTPS (`graph.microsoft.com/v1.0/users/attendance-response@ramboll.com/sendMail`).
- **Cost**: 0 Extra Cost! Included in Ramboll's existing Microsoft 365 E3/E5 subscription.

---

## ⚙️ 2. Step-by-Step Configuration Runbook

1. **Step 1: Create Shared Mailbox in M365 Admin Center (`admin.microsoft.com`)**:
   - Create shared mailbox `attendance-response@ramboll.com` (Requires $0 extra licensing!).

2. **Step 2: Grant `Mail.Send` Application Permission in Entra ID (`entra.microsoft.com`)**:
   - Under *API permissions* &rarr; Add Microsoft Graph Application Permission `Mail.Send` & Grant Admin Consent.

3. **Step 3: Update `appsettings.json`**:
   ```json
   {
     "MailSettings": {
       "Provider": "MicrosoftGraph",
       "SenderEmail": "attendance-response@ramboll.com",
       "SenderDisplayName": "Bkran Group Connect Attendance System",
       "UseGraphApi": true
     }
   }
   ```
