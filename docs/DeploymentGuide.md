# Enterprise Deployment & Operations Guide

> **Document ID**: DEP-2026-ENT-005  
> **Platform Target**: Vercel Serverless / Azure App Service  

---

## 1. Cloud Deployment Configuration

### Vercel Deployment Settings
- **Framework Preset**: Other (ASP.NET Core Web API / Serverless)
- **Environment Variables**:
  - `PORT`: `8080` (Dynamic binding)
  - `TelemetrySettings__UseMockTelemetry`: `true` (Standalone Demo Mode) or `false` (Live Microsoft Graph API)
  - `Smtp__Host`: `smtp.gmail.com`
  - `Smtp__Port`: `587`
  - `Smtp__Username`: `bharathkannan1154@gmail.com`
  - `Smtp__Password`: `[Your-16-Char-App-Password]`

---

## 2. Security Runbook & Incident Response

### Auditing & Incident Playbook (`Cybersecurity_Runbook.md`)
1. **Impossible Travel Alert**: Triggered when an employee registers network sessions in two distant cities within 30 minutes.
2. **Unmanaged Device Telemetry Alert**: Triggered when an event arrives from a non-Intune device.
3. **Session Revocation**: System administrators can revoke active sessions via `/Auth/Logout` or `/api/enterprise/sessions`.
