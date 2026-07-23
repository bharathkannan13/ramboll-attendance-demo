# Ramboll Smart Attendance - Enterprise SSO & Network Integration Architecture

This document outlines the technical requirements, configurations, and architecture needed to scale the Smart Attendance System from this local POC prototype to a production environment supporting 19,000+ hybrid users.

---

## 1. Cisco ISE (Identity Services Engine) Network Presence Integration
To achieve zero-touch, automated presence detection at the corporate office without requiring a local background client service on every laptop, Cisco ISE can act as the primary presence sensor.

### Architecture Flow
```
[User Laptop] ──(Connects to Wi-Fi/802.1X)──> [Cisco ISE]
                                                  │
                                       (Syslog Event / Webhook)
                                                  │
                                                  ▼
                                       [Smart Attendance API]
```

### Technical Requirements
1. **ISE Syslog / Webhook Feed**: Configure Cisco ISE to dispatch SNMP Traps or Syslog Events to our backend API whenever a machine successfully authenticates on the corporate SSID (e.g. `Ramboll_Corporate`) or is assigned an IP via DHCP.
2. **Device Registration (MAB)**: The database `Device_Master` maps the employee's network interface MAC Address. When Cisco ISE triggers the connection webhook:
   ```json
   {
     "mac_address": "AA-BB-CC-DD-EE-FF",
     "event": "SessionStarted",
     "nas_ip": "10.120.2.1",
     "timestamp": "2026-07-17T17:15:00Z"
   }
   ```
   Our Web API resolves the MAC Address, identifies the employee, and logs the `WAKE/ONLINE` event automatically.
3. **Audit & Scope**: This removes client agent dependencies, ensuring 100% compliance across both corporate and personal BYOD laptops.

---

## 2. Microsoft Entra ID (Azure AD) Single Sign-On
For seamless cloud-native SSO authentication in Chrome, Edge, and mobile portals, the system will integrate with Microsoft Entra ID using OAuth 2.0 / OpenID Connect (OIDC).

### Active Directory parameters to request from IT
To register the application in the Azure Portal, you will need to ask the Ramboll Active Directory Team for an **App Registration** with the following values:

| Parameter | Description | Example / Value |
| :--- | :--- | :--- |
| **Tenant ID** | Directory ID of the Ramboll Azure AD tenant. | `12345678-abcd-1234-abcd-12345678cdef` |
| **Client ID** | Application ID generated for the portal. | `87654321-dcba-4321-dcba-87654321fedc` |
| **Authority** | Microsoft Login endpoint URL. | `https://login.microsoftonline.com/{TenantID}` |
| **Redirect URI** | Allowed response URL after a successful login. | `https://attendance.ramboll.com/signin-oidc` |
| **API Scopes** | Permissions required. | `api://{ClientID}/Attendance.Write`, `User.Read` |

### Code Transition (Simulated SSO to Entra ID)

#### POC (Simulated SSO Header)
```csharp
// Read domain login directly from custom headers in TelemetryController
if (Request.Headers.TryGetValue("X-SSO-Identity", out var ssoIdentity)) { ... }
```

#### Production (Entra ID JWT Validation)
We will replace custom headers with Microsoft's official OAuth identity middleware:
```csharp
// Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// TelemetryController.cs
[Authorize]
[HttpPost("log")]
public async Task<IActionResult> LogTelemetry([FromBody] TelemetryEvent requestEvent)
{
    // User UPN is retrieved securely from the validated JWT token claims
    string? userUpn = User.Identity?.Name; // e.g. user@ramboll.com
    ...
}
```

---

## 3. Hybrid Work Policy Reporting (Manager Dashboard)
To verify the weekly "3 days in office" corporate mandate:
1. **Dynamic Shift Merging**: Telemetry logs are grouped by `Work_Date` and `Employee_ID`.
2. **Aggregated Pivot Tables**: Managers can query a weekly pivot showing the total count of days classified as `PRESENT` (>= 8 hours on Ramboll Network) per employee.
3. **Automated Notification**: A scheduled C# worker service runs every Friday at 17:00, compiling the weekly list of employees who failed to meet the 3-day threshold and emailing a summarized report directly to the department manager.
