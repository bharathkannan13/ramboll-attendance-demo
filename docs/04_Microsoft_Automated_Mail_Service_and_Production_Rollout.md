# Stage 4 Guide: Microsoft Automated Mail System & Production Rollout

> **Document ID**: STAGE-04-EMAIL-ROLLOUT-2026  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Scope**: Exact Click-by-Click Portal Actions for Shared Mailbox & Entra ID Mail.Send Permissions

---

## 🖱️ PART A: Exact Clicks in Microsoft 365 Admin Center (`admin.microsoft.com`)

1. Open **[admin.microsoft.com](https://admin.microsoft.com)** & sign in as Global Admin.
2. In left menu: **Teams & groups** &rarr; **Shared mailboxes**.
3. Click **+ Add a shared mailbox** button at the top.
4. Name: `Bkran Attendance System` | Email: `attendance-response` | Domain: `@ramboll.com`.
5. Click **Save changes** at the bottom.

---

## 🖱️ PART B: Exact Clicks in Azure Entra ID Portal (`entra.microsoft.com`)

1. Open **[entra.microsoft.com](https://entra.microsoft.com)**.
2. Left menu: **Applications** &rarr; **App registrations** &rarr; Select `Bkran-Attendance-Engine`.
3. Left menu: **API permissions** &rarr; Click **+ Add a permission**.
4. Select **Microsoft Graph** &rarr; Select **Application permissions**.
5. Search `Mail.Send` &rarr; Check **`Mail.Send`** &rarr; Click **Add permissions**.
6. Click **Grant admin consent for Ramboll** &rarr; Click **Yes** on confirmation pop-up.
7. Verify status column turns **Green Checkmark** saying *"Granted for Ramboll"*.
