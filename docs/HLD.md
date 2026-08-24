# High-Level Architecture Design (HLD)

> **Document ID**: HLD-2026-ENT-001  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics & Hybrid Workforce Platform)  
> **Classification**: CONFIDENTIAL / INTERNAL  
> **Owner**: Technical Architecture & Cyber Security Teams  

---

## 1. System Overview & Architectural Principles

Bkran Group Connect is a multi-tier, high-concurrency enterprise attendance analytics solution. It automates hybrid workforce presence tracking by ingesting telemetry from the **Microsoft 365 Ecosystem** (Entra ID, Microsoft Intune, and Defender for Endpoint) without requiring physical badge swipes.

### Core Architectural Principles
- **Zero Friction**: Automatic physical presence detection via SSID & Bitwise CIDR Subnet matching.
- **Strict Data Localization**: Filtered strictly for **India Regional Offices** (Chennai, Bangalore, Mumbai, Pune, Delhi, Noida, Hyderabad, Gurugram). Non-India global accounts are excluded.
- **Defence-in-Depth Security**: Role-Based Access Control (RBAC), security audit logging, device compliance verification, and encrypted API logging.

---

## 2. High-Level Component Diagram

```
+-----------------------------------------------------------------------------------+
|                            PRESENTATION & USER LAYER                              |
|   +-----------------------+   +-----------------------+   +-------------------+   |
|   | Admin Executive       |   | People Manager        |   | Auth & Quick      |   |
|   | Command Center        |   | Console               |   | Switch Portal     |   |
|   +-----------------------+   +-----------------------+   +-------------------+   |
+-----------------------------------------+-----------------------------------------+
                                          |
                                          v
+-----------------------------------------------------------------------------------+
|                             REST API CONTROLLER LAYER                             |
|  +-------------------+  +-------------------+  +-----------------+  +----------+  |
|  | AdminController   |  | ManagerController |  | AuthController  |  | Enterprise| |
|  +-------------------+  +-------------------+  +-----------------+  +----------+  |
+-----------------------------------------+-----------------------------------------+
                                          |
                                          v
+-----------------------------------------------------------------------------------+
|                                CORE ENGINE LAYER                                  |
|  +------------------+  +-------------------+  +---------------+  +-------------+  |
|  | NetworkClassifier|  | SessionManager    |  | AttendanceEngine| | OrgHierarchy|  |
|  +------------------+  +-------------------+  +---------------+  +-------------+  |
+-----------------------------------------+-----------------------------------------+
                                          |
                                          v
+-----------------------------------------------------------------------------------+
|                                 18-TABLE DATA LAYER                               |
|  +-----------------------------------------------------------------------------+  |
|  | EF Core 8.0 (SqlServer / InMemory / SQLite)                                 |  |
|  | 18 Enterprise Tables (Employee_Master, Attendance_Log, Audit, Risk, etc.)   |  |
|  +-----------------------------------------------------------------------------+  |
+-----------------------------------------------------------------------------------+
```

---

## 3. Key Subsystems & Functional Scope

1. **Governance & Ownership Subsystem**: Manages business and technical ownership (`Application_Ownership`).
2. **RBAC Subsystem**: Enforces role hierarchy (`Admin`, `HR`, `Manager`, `Employee`, `Security Analyst`).
3. **Security Audit & Session Tracking**: Records user logins, session lifetimes, and high-risk administrative operations (`Security_Audit_Log`, `Login_Session_Log`).
4. **Device & Telemetry Intelligence**: Tracks laptop hostnames, MAC addresses, OS versions, and Intune compliance (`Device_Master`).
5. **Cybersecurity & AI Analytics Engine**: Detects attendance anomalies (impossible travel, suspicious login patterns) and forecasts occupancy (`Attendance_Risk_Log`, `Analytics_Log`).
