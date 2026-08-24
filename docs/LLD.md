# Low-Level Software Design (LLD)

> **Document ID**: LLD-2026-ENT-002  
> **System Name**: Bkran Group Connect  
> **Classification**: CONFIDENTIAL / TECHNICAL  

---

## 1. Class Structure & Subsystem Interfaces

### Network Classification Subsystem (`NetworkClassifier.cs`)
- **`ClassifyNetwork(string ipAddress, string ssid)`**: Correlates client telemetry against corporate SSIDs and CIDR ranges.
- **Bitwise Subnet Calculator**: Evaluates IP addresses against subnet masks (e.g. `10.100.0.0/16`) to determine physical office branch.

```
       +-------------------------+
       |   INetworkClassifier    |
       +-------------------------+
                    |
                    v
       +-------------------------+
       |    NetworkClassifier    |
       |  - MatchSSID()          |
       |  - BitwiseSubnetMatch() |
       +-------------------------+
```

---

## 2. Session Lifecycle & Multi-Device Merging

When telemetry events arrive from multiple devices owned by an employee, `SessionManager.cs` processes them according to the following algorithm:

1. **Intune Compliance Gate**: Verify `IsManaged == true` and `ComplianceStatus == Compliant`. If invalid, discard event.
2. **30-Minute Grace Window**: If a new connection event occurs within 30 minutes of the previous disconnect, extend the active session without penalty.
3. **Session Merge**: At End-of-Day (11:59 PM IST), calculate net physical office hours from non-overlapping intervals.

---

## 3. Error Monitoring & Logging Pipeline (`Error_Log`)

- **Unhandled Exception Middleware**: Intercepts unhandled HTTP exceptions.
- **Severity Levels**:
  - `Low`: Minor transient network jitter.
  - `Medium`: Telemetry sync retry warnings.
  - `High`: External Graph API timeout.
  - `Critical`: Database connection loss or authentication failure.
