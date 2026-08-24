# Live Microsoft 365 Connection & Azure Setup Guide

> **Target Platform**: Bkran Group Connect (Enterprise Attendance Analytics & Hybrid Workforce Platform)  
> **Purpose**: Step-by-step instructions on extracting your Azure credentials, toggling live sync on, and configuring network subnets.

---

## 1. Do You Need to Provide Graph API Endpoints?

> [!IMPORTANT]
> **NO! You do NOT need to provide Graph API endpoint URLs.**  
> Standard Microsoft Graph endpoints (`https://graph.microsoft.com/v1.0/users`, `/deviceManagement/managedDevices`, etc.) are **already hardcoded into the system engine (`GraphApiService.cs`)**.  
> You only need to provide **3 simple credentials** from your Azure Portal.

---

## 2. The ONLY 3 Credentials Needed for Live Connection

To connect your live Microsoft ecosystem, you only need:
1. **Azure Tenant ID** (`TenantId`)
2. **Application Client ID** (`ClientId`)
3. **Client Secret Value** (`ClientSecret`)

---

## 3. How to Get These 3 Credentials in Azure Portal (3-Minute Guide)

1. **Sign in to Azure Portal**: Go to [portal.azure.com](https://portal.azure.com)
2. **Create App Registration**: Go to **Microsoft Entra ID** &rarr; **App registrations** &rarr; **New registration** (Name: `Bkran-Attendance-App`)
3. **Copy Tenant ID & Client ID**: Copy **Directory (tenant) ID** & **Application (client) ID** from Overview
4. **Generate Client Secret**: Go to **Certificates & secrets** &rarr; **New client secret** &rarr; Copy Secret **Value**
5. **Grant API Permissions**: Go to **API permissions** &rarr; Add **Microsoft Graph (Application permissions)**:
   - `User.Read.All`
   - `DeviceManagementManagedDevices.Read.All`
   - `SecurityEvents.Read.All`
6. Click **Grant admin consent for [Your Organization]**

---

## 4. How to Provide the Credentials to Me / Turn Live Sync ON

Set the 4 environment variables in Vercel or `appsettings.json`:

```env
TelemetrySettings__UseMockTelemetry=false
AzureAd__TenantId="your-azure-tenant-id-guid"
AzureAd__ClientId="your-app-registration-client-id-guid"
AzureAd__ClientSecret="your-client-secret-value"
```
