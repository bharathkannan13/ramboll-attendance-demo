# Document 1: Source Code Deployment & Azure Integration Guide

> **Document ID**: DOC-01-AZURE-DEPLOY-2026  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Scope**: Official Office Laptops & Azure Cloud Environment  
> **Target Region**: India Regional Offices (**Chennai, Noida, Hyderabad, Gurugram, Bangalore, Mumbai, Pune, Delhi**)

---

## 📍 1. Executive Overview

This guide provides step-by-step instructions for non-technical administrators to pull the approved solution codebase from GitHub onto official Ramboll office laptops, install required prerequisites, and deploy to **Azure App Services + Azure SQL Database**.

---

## 💻 2. Step-by-Step Laptop Setup & Code Pulling

```powershell
# Clone official repository
git clone https://github.com/bharathkannan13/ramboll-attendance-demo.git

# Move into project directory
cd ramboll-attendance-demo/EnterpriseAttendance

# Run local web server
dotnet run --project src/EnterpriseAttendance.Web/EnterpriseAttendance.Web.csproj
```

---

## ☁️ 3. Azure App Service & Azure SQL Database Setup

1. **Azure SQL Database**: Provision SQL Database `sqldb-attendance-production` & copy ADO.NET Connection String.
2. **Azure App Service**: Provision .NET 8.0 Linux/Windows App Service `ramboll-attendance-portal`.
3. **Connect GitHub CI/CD**: Link repository `bharathkannan13/ramboll-attendance-demo` branch `main` in Deployment Center.
4. **Environment Variables**: Set `TelemetrySettings__UseMockTelemetry = false`, `AzureAd__TenantId`, `AzureAd__ClientId`, `AzureAd__ClientSecret`, `ConnectionStrings__DefaultConnection`.

Live Domain Endpoint: **`https://ramboll-attendance-portal.azurewebsites.net`**
