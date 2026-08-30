# Master Technical Requirements & Last Seen Telemetry Correlation Guide

> **Document ID**: DOC-04-TECH-REQUIREMENTS-2026  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Scope**: End-to-End Enterprise Architecture, Correlation Engine, & Configuration Runbook  
> **Target Region**: India Regional Offices (**Chennai, Noida, Hyderabad, Gurugram, Bangalore, Mumbai, Pune, Delhi**)

---

## 📌 1. Comprehensive Technical Requirements Master Catalog

| # | System Component | Requirement Name | Required Parameter / Technical Specification | Purpose & Function |
|---|---|---|---|---|
| 1 | **Identity Service** | **Azure Entra ID Tenant** | `AzureAd__TenantId` (GUID) | Multi-tenant organization identity namespace. |
| 2 | **App Registration** | **Client Application** | `AzureAd__ClientId` & `AzureAd__ClientSecret` | OAuth 2.0 app-only credentials for Microsoft Graph API. |
| 3 | **Graph API Scope** | **Directory Permissions** | `User.Read.All` | Syncs India employee profiles and reporting lines. |
| 4 | **Graph API Scope** | **Intune Permissions** | `DeviceManagementManagedDevices.Read.All` | Reads managed laptop inventory, compliance, and `lastSyncDateTime`. |
| 5 | **Graph API Scope** | **Defender Permissions** | `SecurityEvents.Read.All` | Ingests device `lastSeen`, Wi-Fi SSIDs, and IP adapter bindings. |
| 6 | **Database Layer** | **Azure SQL Database** | `sqldb-attendance-production` (18 ERD Tables) | Multi-year / decade attendance history persistence. |
| 7 | **Hosting Layer** | **Azure App Service** | .NET 8.0 Linux/Windows App Service (`PaaS`) | High-availability global web hosting (`https://...`). |
| 8 | **Network Engine** | **Subnet CIDR Range** | Bitwise CIDR Shift (`10.100.0.0/16`, `10.101.0.0/16`) | Classifies device IP as **OFFICE** vs **REMOTE WFH**. |
| 9 | **Mail Subsystem** | **Automated Email** | M365 Graph `Mail.Send` / SMTP (`bharathkannan1154@gmail.com`) | Weekly Monday 09:00 AM IST manager summary dispatches. |

---

## ⏱️ 2. Telemetry Correlation & "Last Seen" Calculation Engine

$$\text{Session Start (First Seen)} = \min(\text{Intune } t_{\text{sync}}, \text{Defender } t_{\text{seen}}, \text{Network } t_{\text{connect}})$$

$$\text{Session End (Last Seen)} = \max(\text{Intune } t_{\text{sync}}, \text{Defender } t_{\text{seen}}, \text{Network } t_{\text{disconnect}})$$

---

## 🌐 3. State Network CIDR & Wi-Fi Matching Matrix

| State / Hub | Office City | Corporate Wi-Fi SSID | Office IP Subnet (CIDR) | Network Classification |
|---|---|---|---|---|
| **Tamil Nadu** | Chennai | `Ramboll-CHN-Corporate` | `10.100.0.0/16` | **OFFICE** (Chennai Hub) |
| **NCR / UP** | Noida | `Ramboll-NOI-Corporate` | `10.101.0.0/16` | **OFFICE** (Noida Hub) |
| **Telangana** | Hyderabad | `Ramboll-HYD-Corporate` | `10.102.0.0/16` | **OFFICE** (Hyderabad Hub) |
| **NCR / Haryana** | Gurugram | `Ramboll-GUG-Corporate` | `10.103.0.0/16` | **OFFICE** (Gurugram Hub) |
| **Karnataka** | Bangalore | `Ramboll-BLR-Corporate` | `10.104.0.0/16` | **OFFICE** (Bangalore Hub) |
| **Maharashtra** | Mumbai | `Ramboll-MUM-Corporate` | `10.105.0.0/16` | **OFFICE** (Mumbai Hub) |
| **Maharashtra** | Pune | `Ramboll-PUN-Corporate` | `10.106.0.0/16` | **OFFICE** (Pune Hub) |
| **Delhi** | Delhi | `Ramboll-DEL-Corporate` | `10.107.0.0/16` | **OFFICE** (Delhi Hub) |
| *All States* | *WFH / Home* | *Home / Mobile Hotspot* | `192.168.x.x` / `10.200.x.x` (VPN) | **REMOTE WFH** |
