# Stage 4 Guide: Microsoft Automated Mail System & Production Rollout

> **Document ID**: STAGE-04-EMAIL-ROLLOUT-2026  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Scope**: Microsoft 365 Shared Mailbox & Automated Weekly Email Dispatches

---

1. **Shared Mailbox**: Create free shared mailbox `attendance-response@ramboll.com` in `admin.microsoft.com`.
2. **Graph `Mail.Send` Permission**: Grant Application Permission `Mail.Send` in `entra.microsoft.com`.
3. **Automated Weekly Schedule**: Monday 09:00 AM IST dispatches to managers with attached `.xlsx` spreadsheet and single-click hyperlink to `https://ramboll-attendance-portal.azurewebsites.net/Manager`.
