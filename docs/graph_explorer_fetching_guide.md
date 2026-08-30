# Microsoft Graph Explorer Technical Metrics & Data Fetching Runbook

> **Document ID**: RUNBOOK-2026-GRAPH-FETCH-001  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Primary Readable Tool**: **Microsoft Graph Explorer** (`developer.microsoft.com/graph/graph-explorer`)

---

## 🛠️ 1. The Official Readable Tool: Microsoft Graph Explorer

To inspect and fetch all Microsoft 365 technical information from your official laptop **without writing code**, Microsoft provides an official web application called **Microsoft Graph Explorer**.

---

## 📊 2. Table of Technical Metrics & Graph API Endpoints

| # | Category | Technical Metric Needed | Readable Tool Query Endpoint (Paste in Graph Explorer) | What You Copy / Copy Location in JSON |
|---|---|---|---|---|
| 1 | **Tenant Identity** | **Azure Tenant ID** | `GET https://graph.microsoft.com/v1.0/organization` | Copy the `"id"` GUID value from the top response object. |
| 2 | **Employee Directory** | **India Employees List** | `GET https://graph.microsoft.com/v1.0/users?$filter=country eq 'India'` | Inspect array of users; verify `displayName`, `mail`, `jobTitle`, `department`, `officeLocation`. |
| 3 | **Org Hierarchy** | **Manager Direct Reports** | `GET https://graph.microsoft.com/v1.0/users/{user-id}/directReports` | Verify direct reports array for people managers. |
| 4 | **Device Inventory** | **Intune Managed Laptops** | `GET https://graph.microsoft.com/v1.0/deviceManagement/managedDevices` | Copy/verify `"deviceName"`, `"operatingSystem"`, `"complianceState"` (`"compliant"`). |
| 5 | **Network Telemetry** | **Defender Network Events** | `GET https://graph.microsoft.com/v1.0/security/alerts` | Verify connected Wi-Fi SSIDs, IP address subnets, and heartbeat timestamps. |

---

## 👣 3. Step-by-Step Non-Technical Execution Steps

1. Open Chrome/Edge & go to **[developer.microsoft.com/graph/graph-explorer](https://developer.microsoft.com/graph/graph-explorer)**.
2. Click **Sign in to Graph Explorer** in top right & sign in with Ramboll account.
3. Paste each URL into the query bar and click **Run Query**.
4. Inspect/copy the returned JSON data values.
