# Master Pre-Pilot Deployment Playbook (Strict 100% India-Only Scope)

> **Document ID**: PLAYBOOK-2026-INDIA-STRICT-ONLY  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Regional Policy**: **Strict 100% India Scope Enforcement (Zero Global / Non-India Exposure)**  
> **Rule**: **Only employees and managers located physically inside India offices receive emails or access the portal.**

---

## 🇮🇳 Strict 100% India Regional Scope Policy

1. **Zero Non-India Email Dispatches**: `NotificationServices.cs` verifies the manager's `OfficeLocation` before dispatch. If outside India (e.g. Denmark, Germany, UK, US), the service skips sending email completely. Non-India managers will **NEVER receive automated emails**.
2. **Zero Non-India Telemetry Ingestion**: `GraphApiService.cs` uses OData filter `$filter=country eq 'India'` for Indian hubs (**Chennai, Noida, Hyderabad, Gurugram, Bangalore, Mumbai, Pune, Delhi**). Global accounts are **100% EXCLUDED**.
3. **Zero Non-India Portal SSO Access**: OpenID Connect SSO authenticates `officeLocation`. Non-India users are denied access and redirected to an **"Access Denied: Restricted to Ramboll India Personnel"** page.
