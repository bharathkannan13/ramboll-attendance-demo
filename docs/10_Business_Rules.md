# Enterprise Attendance & Workforce Analytics Platform
## Business Rules Specification Document

### 1. Executive Summary

The Business Rules Specification Document (BRSD) defines the core business logic, policies, and constraints governing the Enterprise Attendance & Workforce Analytics Platform. This system is designed to provide agentless, accurate tracking of Office Presence for employees located in Indian offices (Chennai, Noida, Hyderabad, Gurugram, and Bangalore). 

This document serves as the definitive source of truth for all business rules related to network classification, session management, attendance calculation, policy compliance, organizational hierarchy, role-based access control, notifications, data retention, and calendar configurations. These rules ensure that the platform correctly interprets telemetry data, handles edge cases gracefully, and aligns with the organization's hybrid work policies without infringing on user privacy or requiring endpoint agents.

---

### 2. Business Rules Catalog

#### Category 1: Network Classification Rules (BR-NW)

| Rule ID | Statement | Priority | Configurable | Default |
|---------|-----------|----------|--------------|---------|
| **BR-NW-001** | Device connected to configured corporate SSID SHALL be classified as OFFICE. | Must | Yes | N/A |
| **BR-NW-002** | Device connected to configured corporate subnet SHALL be classified as OFFICE. | Must | Yes | N/A |
| **BR-NW-003** | Device connected via VPN with IP NOT in corporate subnet SHALL be classified as REMOTE. | Must | No | N/A |
| **BR-NW-004** | Device connected via VPN with IP IN corporate subnet and SSID matches SHALL be classified as OFFICE. | Must | No | N/A |
| **BR-NW-005** | Device on unknown network SHALL be classified as UNKNOWN and flagged for admin review. | Must | No | N/A |
| **BR-NW-006** | Network classification SHALL be configurable per office location. | Must | Yes | N/A |
| **BR-NW-007** | Multiple network identifiers per office SHALL be supported. | Must | Yes | N/A |
| **BR-NW-008** | Network identifiers MUST NOT be overlapping across different office locations. | Must | No | N/A |
| **BR-NW-009** | Updates to network configurations SHALL apply retroactively to active sessions only, not historical data. | Should | No | N/A |
| **BR-NW-010** | Any network mismatch between IPv4 and IPv6 subnets SHALL default to the more secure classification (UNKNOWN). | Must | No | N/A |
| **BR-NW-011** | Guest Wi-Fi networks SHALL be classified as REMOTE. | Must | Yes | N/A |
| **BR-NW-012** | Wired LAN connections on corporate VLANs SHALL be classified as OFFICE. | Must | Yes | N/A |
| **BR-NW-013** | MAC address spoofing detection rules SHALL invalidate the session if triggered. | Could | Yes | Off |
| **BR-NW-014** | A device reporting multiple active network adapters SHALL prioritize the OFFICE classification if one adapter matches. | Must | No | N/A |
| **BR-NW-015** | Subnet masks for matching SHALL support CIDR notation (e.g., /24, /16). | Must | Yes | N/A |
| **BR-NW-016** | External IP address matching SHALL be used as a fallback if internal IP is not reported. | Should | Yes | N/A |
| **BR-NW-017** | The system SHALL periodically validate configured network identifiers against Azure network topology. | Could | Yes | Weekly |
| **BR-NW-018** | Manual override of classification SHALL be logged with admin ID and timestamp. | Must | No | N/A |
| **BR-NW-019** | Subnets identified as IoT or Server VLANs SHALL NOT trigger OFFICE presence for standard users. | Must | Yes | N/A |
| **BR-NW-020** | Network classification rules SHALL be evaluated in a specific priority order: SSID, then Wired Subnet, then External IP. | Must | Yes | N/A |

**Detailed Example: BR-NW-001**
- **Business Justification:** Connecting to the corporate Wi-Fi is a strong indicator of physical presence.
- **Edge Cases:** Device connects to corporate Wi-Fi from the parking lot. (Considered Office).
- **Acceptance Criteria:** Given an event with an SSID matching `RAMBOLL-CORP`, the system creates or updates an OFFICE session.

#### Category 2: Session Management Rules (BR-SM)

| Rule ID | Statement | Priority | Configurable | Default |
|---------|-----------|----------|--------------|---------|
| **BR-SM-001** | A session starts when a device first reports telemetry from an OFFICE or REMOTE network for the day. | Must | No | N/A |
| **BR-SM-002** | Telemetry updates within the grace period SHALL extend the current session's Last Seen time. | Must | Yes | 15 mins |
| **BR-SM-003** | If no telemetry is received for a period exceeding the grace period, the session SHALL be closed. | Must | Yes | 30 mins |
| **BR-SM-004** | A new session SHALL be created if telemetry is received after a session is closed. | Must | No | N/A |
| **BR-SM-005** | Multiple sessions for the same user on the same day SHALL be merged into a daily aggregate. | Must | No | N/A |
| **BR-SM-006** | Overlapping sessions from multiple devices SHALL be merged, calculating distinct time only once. | Must | No | N/A |
| **BR-SM-007** | A session changing from OFFICE to REMOTE SHALL close the OFFICE session and open a REMOTE session. | Must | No | N/A |
| **BR-SM-008** | Short disconnects (under 5 mins) across network boundaries SHALL be ignored. | Should | Yes | 5 mins |
| **BR-SM-009** | End-of-day processing SHALL forcibly close any open sessions at midnight local time. | Must | No | N/A |
| **BR-SM-010** | Sessions with a total duration of less than a minimum threshold SHALL be discarded to prevent bounce noise. | Must | Yes | 10 mins |
| **BR-SM-011** | Device sleep/hibernate events SHALL immediately close the active session. | Must | No | N/A |
| **BR-SM-012** | Device wake events SHALL initiate a new session evaluation. | Must | No | N/A |
| **BR-SM-013** | If a device jumps between two Indian offices on the same day, both OFFICE locations SHALL be logged. | Must | No | N/A |
| **BR-SM-014** | Roaming between access points within the same office SHALL NOT trigger a new session. | Must | No | N/A |
| **BR-SM-015** | Sessions crossing the midnight boundary SHALL be split into two daily records. | Must | No | N/A |
| **BR-SM-016** | Missed heartbeat events due to network congestion SHALL be retroactively filled if subsequent heartbeat arrives within 1 hour. | Should | Yes | 1 hour |
| **BR-SM-017** | Manual session edits by HR SHALL override automated telemetry for that day. | Must | No | N/A |
| **BR-SM-018** | Sessions marked as anomalous (e.g., impossible travel) SHALL be flagged but retained. | Must | No | N/A |
| **BR-SM-019** | Merged session calculation SHALL use the earliest First Seen and latest Last Seen. | Must | No | N/A |
| **BR-SM-020** | Total active time SHALL be the sum of all closed session durations minus overlaps. | Must | No | N/A |

#### Category 3: Attendance Calculation Rules (BR-AC)

| Rule ID | Statement | Priority | Configurable | Default |
|---------|-----------|----------|--------------|---------|
| **BR-AC-001** | First Seen is the start time of the earliest OFFICE session for the calendar day. | Must | No | N/A |
| **BR-AC-002** | Last Seen is the end time of the latest OFFICE session for the calendar day. | Must | No | N/A |
| **BR-AC-003** | Total Office Hours is the sum of durations of all distinct OFFICE sessions. | Must | No | N/A |
| **BR-AC-004** | Total Remote Hours is the sum of durations of all distinct REMOTE sessions. | Must | No | N/A |
| **BR-AC-005** | Remote session hours SHALL NOT be included in Office Presence Hours. | Must | No | N/A |
| **BR-AC-006** | Weekly attendance SHALL count the number of distinct calendar days with at least one OFFICE session exceeding minimum duration. | Must | Yes | 4 hours |
| **BR-AC-007** | A half-day office presence SHALL be counted if total office hours are between 2 and 4 hours. | Must | Yes | 2-4 hrs |
| **BR-AC-008** | Full-day office presence requires minimum office hours. | Must | Yes | >4 hours |
| **BR-AC-009** | Monthly attendance aggregation SHALL be based on standard calendar months. | Must | No | N/A |
| **BR-AC-010** | Weekly aggregation SHALL start on Monday and end on Sunday. | Must | No | N/A |
| **BR-AC-011** | Leave days (approved in HRMS) SHALL be excluded from the expected attendance baseline. | Must | No | N/A |
| **BR-AC-012** | Public holidays SHALL be excluded from the expected attendance baseline. | Must | No | N/A |
| **BR-AC-013** | If an employee works on a public holiday from the office, it SHALL be logged as extra presence. | Should | No | N/A |
| **BR-AC-014** | Missing data days SHALL be treated as REMOTE by default. | Must | No | N/A |
| **BR-AC-015** | Daily attendance reports SHALL be generated at 2:00 AM IST for the previous day. | Must | Yes | 2:00 AM |
| **BR-AC-016** | Core hours overlap SHALL be calculated to show presence between 10:00 AM and 4:00 PM. | Could | Yes | 10-16 |
| **BR-AC-017** | Overtime SHALL NOT be calculated by this system. | Must | No | N/A |
| **BR-AC-018** | Weekly average calculation SHALL use rolling 4-week periods for trend analysis. | Should | No | N/A |
| **BR-AC-019** | System SHALL define a Day as 00:00:00 to 23:59:59 IST. | Must | No | N/A |
| **BR-AC-020** | Attendance records older than 24 hours are locked and require Admin override to change. | Must | No | N/A |

#### Category 4: Hybrid Policy Compliance Rules (BR-HP)

| Rule ID | Statement | Priority | Configurable | Default |
|---------|-----------|----------|--------------|---------|
| **BR-HP-001** | Target office days per week SHALL be defined globally. | Must | Yes | 3 days |
| **BR-HP-002** | Target office days SHALL be adjustable per department. | Must | Yes | N/A |
| **BR-HP-003** | Status: MET is assigned if actual office days >= target office days. | Must | No | N/A |
| **BR-HP-004** | Status: PARTIALLY MET is assigned if actual office days == (target - 1). | Must | No | N/A |
| **BR-HP-005** | Status: NON-COMPLIANT is assigned if actual office days < (target - 1). | Must | No | N/A |
| **BR-HP-006** | Compliance calculations SHALL adjust the target proportionally for partial weeks (e.g., leaves, holidays). | Must | No | N/A |
| **BR-HP-007** | A 4-week rolling compliance average SHALL be calculated for managers. | Must | No | N/A |
| **BR-HP-008** | Exemptions to the hybrid policy SHALL be configurable per user (e.g., permanent remote). | Must | No | N/A |
| **BR-HP-009** | Exempted users SHALL always show as N/A in compliance reports. | Must | No | N/A |
| **BR-HP-010** | Monthly compliance scores SHALL be generated on the 1st of every month. | Must | No | N/A |

#### Category 5: Device & Compliance Rules (BR-DC)

| Rule ID | Statement | Priority | Configurable | Default |
|---------|-----------|----------|--------------|---------|
| **BR-DC-001** | Only managed Intune devices SHALL be tracked for attendance. | Must | No | N/A |
| **BR-DC-002** | Devices marked as non-compliant in Intune SHALL still report telemetry if capable. | Must | No | N/A |
| **BR-DC-003** | Personal devices (BYOD) without Intune MDM SHALL NOT be tracked. | Must | No | N/A |
| **BR-DC-004** | When an employee replaces a device, history from the old device SHALL merge seamlessly. | Must | No | N/A |
| **BR-DC-005** | Primary device heuristics SHALL identify the most frequently used device for conflict resolution. | Should | No | N/A |
| **BR-DC-006** | Mobile devices (iOS/Android) SHALL be excluded from tracking unless specified. | Must | Yes | Excluded |
| **BR-DC-007** | Virtual Desktop Infrastructure (VDI) sessions SHALL be classified as REMOTE regardless of network. | Must | No | N/A |
| **BR-DC-008** | Devices with spoofed telemetry timestamps SHALL be flagged and data discarded. | Must | No | N/A |
| **BR-DC-009** | Shared devices (kiosks) SHALL NOT be used for individual attendance tracking. | Must | No | N/A |
| **BR-DC-010** | Disabling telemetry collection locally SHALL mark the user as Missing Data / Remote. | Must | No | N/A |

#### Category 6: Organizational Hierarchy Rules (BR-OH)

| Rule ID | Statement | Priority | Configurable | Default |
|---------|-----------|----------|--------------|---------|
| **BR-OH-001** | Org hierarchy SHALL be auto-synced from Microsoft Entra ID. | Must | No | N/A |
| **BR-OH-002** | The sync process SHALL run daily to capture reporting line changes. | Must | Yes | Daily |
| **BR-OH-003** | Only employees with a defined location in India SHALL be imported. | Must | No | India |
| **BR-OH-004** | A manager's view SHALL recursively include all indirect reports. | Must | No | N/A |
| **BR-OH-005** | If a manager leaves, their direct reports SHALL roll up to the skip-level manager temporarily. | Must | No | N/A |
| **BR-OH-006** | Department names SHALL be standardized based on Entra ID attributes. | Must | No | N/A |
| **BR-OH-007** | Matrix management (multiple managers) is NOT supported for attendance rollups. | Must | No | N/A |
| **BR-OH-008** | Employees without a manager in Entra ID SHALL be flagged as orphans for HR review. | Must | No | N/A |
| **BR-OH-009** | Transferring an employee between departments SHALL lock historical data to the old department. | Must | No | N/A |
| **BR-OH-010** | The system SHALL handle circular reporting structures gracefully (log error, skip loop). | Must | No | N/A |

#### Category 7: RBAC & Access Rules (BR-RB)

| Rule ID | Statement | Priority | Configurable | Default |
|---------|-----------|----------|--------------|---------|
| **BR-RB-001** | Employees SHALL only view their own attendance data. | Must | No | N/A |
| **BR-RB-002** | Managers SHALL view data for themselves and all direct/indirect reports. | Must | No | N/A |
| **BR-RB-003** | Department Heads SHALL view data for their entire department. | Must | No | N/A |
| **BR-RB-004** | HR SHALL view data for all Indian employees. | Must | No | N/A |
| **BR-RB-005** | Administrators SHALL have full system access including configurations. | Must | No | N/A |
| **BR-RB-006** | Executive Management SHALL have access to aggregate anonymized dashboards. | Must | No | N/A |
| **BR-RB-007** | Access to raw telemetry logs SHALL be restricted to Administrators only. | Must | No | N/A |
| **BR-RB-008** | All administrative actions SHALL be logged in an audit trail. | Must | No | N/A |
| **BR-RB-009** | Role assignments SHALL be managed via Entra ID Security Groups. | Must | No | N/A |
| **BR-RB-010** | Session timeouts for the web portal SHALL enforce re-authentication. | Must | Yes | 30 mins |
| **BR-RB-011** | API access SHALL require valid OAuth 2.0 Bearer tokens. | Must | No | N/A |
| **BR-RB-012** | Service Accounts for background jobs SHALL have a restricted internal role. | Must | No | N/A |
| **BR-RB-013** | Delegated access (e.g., EA acting for VP) SHALL NOT be supported natively (use Entra). | Must | No | N/A |
| **BR-RB-014** | Data export functions SHALL be restricted to HR and Admins. | Must | No | N/A |
| **BR-RB-015** | Attempted unauthorized access SHALL trigger a security alert. | Must | No | N/A |

#### Category 8: Notification Rules (BR-NT)

| Rule ID | Statement | Priority | Configurable | Default |
|---------|-----------|----------|--------------|---------|
| **BR-NT-001** | Weekly summary emails SHALL be sent to managers on Monday 9:00 AM IST. | Must | Yes | Mon 9AM |
| **BR-NT-002** | Non-compliance alerts SHALL be sent to employees if target is not met for 2 consecutive weeks. | Should | Yes | N/A |
| **BR-NT-003** | Non-compliance alerts SHALL be CC'd to managers. | Should | Yes | N/A |
| **BR-NT-004** | Email templates SHALL be customizable via the Admin portal. | Must | No | N/A |
| **BR-NT-005** | System outage notifications SHALL be sent to Administrators. | Must | No | N/A |
| **BR-NT-006** | Employees achieving 100% compliance for a month SHALL receive a positive reinforcement email. | Could | Yes | Off |
| **BR-NT-007** | Unrecognized network alerts SHALL be batched daily to Admins. | Must | Yes | Daily |
| **BR-NT-008** | End-of-month reports SHALL be distributed to Department Heads on the 2nd of the month. | Must | Yes | 2nd |
| **BR-NT-009** | Opt-out for purely informational notifications SHALL be supported. | Should | No | N/A |
| **BR-NT-010** | All emails SHALL be routed through Microsoft Graph API / Exchange Online. | Must | No | N/A |
| **BR-NT-011** | Notifications SHALL respect user timezones (default IST). | Must | No | IST |
| **BR-NT-012** | Failed notification deliveries SHALL be logged and retried up to 3 times. | Must | No | 3 |
| **BR-NT-013** | High-priority system alerts SHALL bypass opt-out settings. | Must | No | N/A |
| **BR-NT-014** | HR alerts for orphan users SHALL be generated post-Entra sync. | Must | No | N/A |
| **BR-NT-015** | Links within emails SHALL point to the specific authenticated dashboard views. | Must | No | N/A |

#### Category 9: Data Retention Rules (BR-DR)

| Rule ID | Statement | Priority | Configurable | Default |
|---------|-----------|----------|--------------|---------|
| **BR-DR-001** | Raw endpoint telemetry SHALL be retained for 30 days. | Must | Yes | 30 Days |
| **BR-DR-002** | Processed daily attendance sessions SHALL be retained for 1 year. | Must | Yes | 1 Year |
| **BR-DR-003** | Aggregated weekly/monthly compliance data SHALL be retained for 3 years. | Must | Yes | 3 Years |
| **BR-DR-004** | Audit logs for administrative actions SHALL be retained indefinitely. | Must | No | N/A |
| **BR-DR-005** | Deletion of data upon employee departure SHALL follow standard corporate data lifecycle policies. | Must | No | N/A |

#### Category 10: Indian Working Calendar Rules (BR-IW)

| Rule ID | Statement | Priority | Configurable | Default |
|---------|-----------|----------|--------------|---------|
| **BR-IW-001** | Standard working hours SHALL be 9:30 AM to 6:30 PM IST. | Must | Yes | 09:30-18:30|
| **BR-IW-002** | Standard work week SHALL be Monday to Friday. | Must | Yes | Mon-Fri |
| **BR-IW-003** | Saturdays and Sundays SHALL be classified as Non-working days. | Must | Yes | Sat/Sun |
| **BR-IW-004** | A configurable Holiday Calendar SHALL define Indian public and regional holidays. | Must | No | N/A |
| **BR-IW-005** | Regional holidays SHALL be mapped to specific office locations (e.g., Ugadi in Bangalore/Hyderabad, Pongal in Chennai). | Must | No | N/A |
| **BR-IW-006** | Office presence on a Non-working day or Holiday SHALL count towards weekly totals as bonus days. | Must | No | N/A |
| **BR-IW-007** | Core hours expectations SHALL be evaluated only during standard working hours. | Should | No | N/A |
| **BR-IW-008** | Half-days declared by HR (e.g., pre-festival) SHALL halve the required office duration for that day. | Must | No | N/A |
| **BR-IW-009** | Shift workers with non-standard hours SHALL require an exception profile in the system. | Must | No | N/A |
| **BR-IW-010** | End-of-year mandatory shutdowns SHALL automatically adjust hybrid compliance targets to zero. | Must | No | N/A |

---

### 3. Business Rules Decision Tables

#### Decision Table 1: Network Session Classification

| Condition: Corporate SSID Match | Condition: Corporate Subnet Match | Condition: VPN Active | Action: Session Type | Action: Flag for Review |
|---------------------------------|-----------------------------------|-----------------------|----------------------|-------------------------|
| TRUE | TRUE | FALSE | **OFFICE** | NO |
| TRUE | FALSE | FALSE | **OFFICE** | NO |
| FALSE | TRUE | FALSE | **OFFICE** | NO |
| FALSE | FALSE | TRUE | **REMOTE** | NO |
| TRUE | TRUE | TRUE | **OFFICE** | NO |
| FALSE | FALSE | FALSE | **UNKNOWN** | YES |

#### Decision Table 2: Weekly Hybrid Compliance

| Condition: Weekly Target | Condition: Adjusted Target (Leaves) | Condition: Actual Office Days | Action: Compliance Status |
|--------------------------|-------------------------------------|-------------------------------|---------------------------|
| 3 | 3 | >= 3 | **MET** |
| 3 | 3 | 2 | **PARTIALLY MET** |
| 3 | 3 | < 2 | **NON-COMPLIANT** |
| 3 | 2 | >= 2 | **MET** |
| 3 | 2 | 1 | **PARTIALLY MET** |
| 3 | 2 | 0 | **NON-COMPLIANT** |

---

### 4. Rules Configuration Management

Business rules in the "Configurable" category can be managed through the Administration Portal. 
- **Storage:** Configurations are stored in the SQL Server `SystemConfigurations` and `NetworkIdentifiers` tables.
- **Modification:** Only Administrators can modify global rules. Department Heads can modify department-specific targets within bounded limits.
- **Auditing:** Every modification is logged using Entity Framework interceptors, recording the previous value, new value, User ID (Entra ID OID), and timestamp.
- **Cache Invalidation:** Updates to rules trigger an immediate Redis cache invalidation for the affected settings to ensure real-time enforcement.

---

### 5. Assumptions, Risks, and Dependencies

**Assumptions**
- Employees connect their primary managed devices to the corporate network when in the office.
- Network infrastructure (SSIDs, Subnets) is stable and changes are communicated to the platform administrators in advance.
- Entra ID data (Manager, Location, Department) is accurate and actively maintained by IT.

**Risks**
- **MAC/IP Spoofing:** Technically savvy users might attempt to spoof network identifiers. Mitigation: Correlate with physical access logs if necessary (out of scope for MVP).
- **Network Outages:** A local office network outage might force users to hot-spot, resulting in REMOTE classification while physically in the office. Mitigation: HR manual overrides.

**Dependencies**
- **Microsoft Intune / Defender for Endpoint:** For providing the agentless device network telemetry.
- **Microsoft Entra ID:** For identity, role, and hierarchical data.
- **Exchange Online:** For notification delivery.
- **Corporate Network Team:** To provide and maintain accurate IP Subnet and SSID whitelists for the Indian offices.
