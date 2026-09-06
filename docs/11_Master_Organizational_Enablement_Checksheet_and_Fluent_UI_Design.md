# Document 11: Master Organizational Enablement Checksheet & Microsoft Fluent UI 3.0 Specification

> **Document ID**: CHECKLIST-2026-ORGANIZATIONAL-ENABLEMENT-001  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Standard**: **100% Organizational Enablement & Microsoft Fluent Design System 3.0 UI/UX**

---

## 📋 PART 1: 100% Organizational Enablement Requirements Checksheet

| Category | Technical Requirement | Verification Tool | Complete (Y/N) |
|---|---|---|---|
| **1. Entra ID App Registration** | Create app `Bkran-Attendance-Engine` in `entra.microsoft.com`; obtain `TenantId`, `ClientId`, and `ClientSecret`. Configure Web SSO Redirect URI `https://ramboll-attendance-portal.azurewebsites.net/signin-oidc`. | Entra ID Portal | [ ] |
| **2. Graph API Permissions** | Grant 4 Application Permissions: `User.Read.All`, `DeviceManagementManagedDevices.Read.All`, `SecurityEvents.Read.All`, `Mail.Send`. Click **Grant admin consent for Ramboll**. | Entra ID Portal | [ ] |
| **3. Shared Mailbox** | Create free shared mailbox `attendance-response@ramboll.com` in `admin.microsoft.com` ($0 extra licensing cost). | M365 Admin Center | [ ] |
| **4. Azure SQL Database** | Provision Azure SQL DB `sqldb-attendance-production` on server `sqlserver-ramboll-india`; obtain connection string. | Azure Portal | [ ] |
| **5. Azure App Service** | Provision .NET 8.0 App Service `ramboll-attendance-portal` & connect GitHub repo `ramboll/attendance-engine` main branch. | Azure Portal | [ ] |
| **6. Subnet CIDR Matrix** | Input corporate Wi-Fi CIDR ranges for Indian hubs (Chennai `10.100.0.0/16`, Noida `10.101.0.0/16`, Hyderabad `10.102.0.0/16`, etc.). | Admin Portal UI | [ ] |

---

## 🎨 PART 2: Microsoft Fluent Design System 3.0 UI/UX Specification

The application user interface has been upgraded to match **Microsoft’s Fluent 3.0 Enterprise Design System**, utilizing official Microsoft web parts, colors, Segoe UI typography, icons, and acrylic glassmorphism.

```css
:root {
  /* Microsoft Core Brand Palette */
  --ms-fluent-blue: #0078D4;          /* Microsoft Communication Blue */
  --ms-fluent-blue-hover: #106EBE;    /* Fluent Blue Hover */
  --ms-fluent-blue-dark: #005A9E;     /* Deep Navy Accent */
  --ms-fluent-cyan: #00A4EF;          /* Microsoft Azure Cyan */
  --ms-fluent-green: #107C41;         /* Microsoft Office Green (In-Office Status) */
  --ms-fluent-orange: #D83B01;        /* Microsoft Office Orange (WFH Status) */
  --ms-fluent-purple: #5C2D91;        /* Microsoft Power BI Purple */
  --ms-fluent-red: #E81123;           /* Microsoft Defender Alert Red */

  /* Dark Canvas & Acrylic Layers */
  --ms-bg-canvas: #0B132B;
  --ms-bg-card: rgba(17, 28, 56, 0.88);
  --ms-border-subtle: rgba(0, 164, 239, 0.22);
}
```
