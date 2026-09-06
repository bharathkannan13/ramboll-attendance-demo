# Document 12: 1-Slide Ramboll IT Technical Requirements & Master AI Prompt

> **Document ID**: PRESENTATION-2026-ONE-SLIDE-RAMBOLL-REQ-001  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Audience**: Ramboll IT Infrastructure Team, Enterprise Architecture, & IT Director  
> **Format**: 1-Slide Executive Requirements Deck + Master AI Prompt

---

## 🤖 Master AI Prompt (Copy-Paste into PowerPoint / Copilot / ChatGPT / Gamma)

```text
Act as an Enterprise Solutions Architect presenting to the Ramboll IT & Infrastructure Management Team. Create a high-impact, professional 1-slide PowerPoint presentation titled 'Technical Prerequisites & Inputs Needed from Ramboll IT' for the Bkran Group Connect Hybrid Attendance Analytics Platform.

Theme Palette: Microsoft Fluent Navy (#0B132B, #0078D4, #F3F2F1, #107C41).

Slide Content:
Header: Technical Prerequisites & Infrastructure Inputs Needed from Ramboll IT
Subtitle: 5 Required Deliverables to Enable 100% Automated Hybrid Attendance Tracking

Grid Layout (5 Clean Cards/Columns):

Card 1: Azure Entra ID App Registration
- Register App: 'Bkran-Attendance-Engine' in Entra ID Portal.
- Deliverables: Azure Tenant ID, Client ID, Client Secret.
- Admin Consent Granted for 4 Application Permissions: User.Read.All, DeviceManagementManagedDevices.Read.All, SecurityEvents.Read.All, Mail.Send.

Card 2: M365 Shared Mailbox
- Deliverable: Free Shared Mailbox 'attendance-response@ramboll.com'.
- Purpose: Dispatches automated weekly Monday 09:00 AM attendance emails to managers at $0 extra licensing cost.

Card 3: Azure Cloud Hosting Provisioning
- Deliverables: Access/Permission to create 1 Azure App Service (.NET 8.0) and 1 Azure SQL Database in South India region.
- Purpose: 10+ year data retention and single-click SSO web console.

Card 4: Regional Network Subnet CIDR Matrix
- Deliverable: List of corporate Wi-Fi CIDR ranges for Indian hubs (Chennai 10.100.0.0/16, Noida 10.101.0.0/16, Hyderabad, Gurugram, Bangalore, etc.).
- Purpose: Bitwise subnet engine matching to classify OFFICE vs REMOTE WFH.

Card 5: Pre-Pilot Testing Manager Email
- Deliverable: 1 Manager Corporate Email for Pre-Pilot Staging.
- Purpose: Graph API auto-discovers reporting team, sends weekly Excel report, and tests Entra ID Single Sign-On (SSO).

Footer Banner: Zero Third-Party Hardware Required • 100% Leverages Existing M365 Licensing • Strict Regional Data Localization.
```
