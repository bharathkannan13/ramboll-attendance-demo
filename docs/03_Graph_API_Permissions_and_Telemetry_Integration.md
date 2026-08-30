# Stage 3 Guide: Graph API Permissions & Telemetry Integration

> **Document ID**: STAGE-03-GRAPH-PERMISSIONS-2026  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Scope**: Microsoft Entra ID App Registration & Telemetry Permissions

---

1. **App Registration**: Register `Bkran-Attendance-Engine` in `entra.microsoft.com` & copy `Application (client) ID` and `Directory (tenant) ID`.
2. **Client Secret**: Create Client Secret under *Certificates & secrets*.
3. **Graph API Permissions**: Add Application Permissions `User.Read.All`, `DeviceManagementManagedDevices.Read.All`, `SecurityEvents.Read.All` & click **Grant admin consent**.
