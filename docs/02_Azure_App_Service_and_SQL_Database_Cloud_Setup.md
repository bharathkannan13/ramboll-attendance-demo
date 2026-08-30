# Stage 2 Guide: Azure App Service & Azure SQL Database Cloud Setup

> **Document ID**: STAGE-02-AZURE-CLOUD-2026  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Scope**: Production Cloud Infrastructure Setup in Azure Portal (`portal.azure.com`)

---

1. **Azure SQL Database**: Provision SQL Database `sqldb-attendance-production` & copy ADO.NET Connection String.
2. **Azure App Service**: Provision .NET 8.0 Linux/Windows App Service `ramboll-attendance-portal`.
3. **Connect GitHub CI/CD**: Link repository `ramboll/attendance-engine` branch `main` in Deployment Center.
4. **Environment Variables**: Set `TelemetrySettings__UseMockTelemetry = false`, `AzureAd__TenantId`, `AzureAd__ClientId`, `AzureAd__ClientSecret`, `ConnectionStrings__DefaultConnection`.

Public Domain Endpoint: **`https://ramboll-attendance-portal.azurewebsites.net`**
