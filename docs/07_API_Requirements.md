# Software Requirements Specification: API Requirements & Design Document

## Enterprise Attendance & Workforce Analytics Platform

---

## 1. Executive Summary

This API Requirements & Design Document specifies the complete Application Programming Interface (API) architecture, endpoints, data models, and integration protocols for the **Enterprise Attendance & Workforce Analytics Platform**. The platform is an agentless attendance tracking system deployed for Indian office locations (Chennai, Noida, Hyderabad, Gurugram, and Bangalore) that determines employee office presence passively via network telemetry.

The APIs defined in this document serve as the critical backend layer, exposing attendance metrics, hierarchy data, system configuration, and business intelligence to frontend interfaces (web dashboard, reporting tools) and enabling integrations with the broader Microsoft 365 ecosystem. The architecture embraces RESTful principles, stringent role-based access controls (RBAC), uniform data exchange formats, and comprehensive API lifecycle management strategies.

This document targets backend engineers, frontend developers, integration specialists, and QA engineers, providing the exhaustive technical blueprint necessary for implementation, testing, and system integration.

---

## 2. API Design Principles

The Enterprise Attendance API adheres strictly to industry-standard best practices to ensure predictability, scalability, maintainability, and ease of integration.

### Core Principles

*   **RESTful Architecture**: The API strictly follows REpresentational State Transfer principles. Resources are identified by URIs, and interactions occur using standard HTTP methods (`GET`, `POST`, `PUT`, `PATCH`, `DELETE`).
*   **Statelessness**: Every request contains all necessary information to process it (e.g., authentication tokens in headers). The server retains no client context between requests.
*   **JSON Only**: All requests (except file uploads) and responses use `application/json`.
*   **Uniform Interface**: Responses follow a standardized schema globally across all endpoints to simplify frontend consumption and error handling.
*   **HATEOAS**: Where practical, responses include hypermedia links to related resources or actions.
*   **Idempotency**: `GET`, `PUT`, and `DELETE` requests are inherently idempotent, ensuring that repeated identical requests produce the same server state.
*   **Resource Naming**: Plural nouns are used for resource URIs (e.g., `/employees` instead of `/employee`).

---

## 3. Authentication & Authorization

Given the sensitive nature of employee tracking and location telemetry, security is paramount. The API relies entirely on the corporate Identity Provider (Microsoft Entra ID).

### Authentication

*   **Protocol**: OAuth 2.0 / OpenID Connect (OIDC).
*   **Token Type**: JSON Web Token (JWT).
*   **Flow**: The frontend authenticates users via MSAL (Microsoft Authentication Library) against Entra ID and obtains an Access Token.
*   **Mechanism**: All protected API endpoints require the JWT to be passed in the `Authorization` HTTP header using the `Bearer` schema.
    ```http
    Authorization: Bearer eyJhbGciOiJSUzI...
    ```

### Authorization (RBAC & ABAC)

Authorization is enforced at two levels: Role-Based Access Control (RBAC) and Attribute-Based Access Control (ABAC), primarily focusing on the Manager Hierarchy.

*   **Roles Supported**:
    *   `Employee`: Read-only access to own data.
    *   `Manager`: Read access to direct and indirect reports' data.
    *   `DepartmentHead`: Read access to all departmental data.
    *   `HR`: Global read access, basic configuration access.
    *   `Administrator`: Full system access, configuration, and manual sync controls.
    *   `ExecutiveManagement`: Global read access to high-level summaries and dashboards.

*   **Hierarchy Enforcement**:
    When a Manager accesses data via the `/api/v1/manager/*` endpoints, the backend API evaluates the caller's Entra ID claims (specifically OID/UPN), queries the organizational hierarchy, and restricts the data response strictly to employees falling within that manager's direct or indirect reporting line. A manager attempting to access a non-report's attendance record (e.g., via `/api/v1/employees/{non-report-id}/attendance`) will receive a `403 Forbidden`.

---

## 4. Base Response Model

To guarantee a predictable development experience for API consumers, **all** API responses (success or failure) strictly adhere to the following wrapper schema.

```json
{
  "success": boolean,      // True if the request was successful, false otherwise
  "data": object | array,  // The actual payload. Null on errors or if no data.
  "message": string,       // Human-readable message (e.g., "Resource created successfully")
  "errors": array,         // Array of error details. Null or empty on success.
  "pagination": object     // Pagination metadata. Null if request is not paginated.
}
```

### Detailed Schema Definitions

**Error Object (`errors` array item)**:
```json
{
  "code": "VALIDATION_ERROR",
  "field": "employeeId",
  "message": "The employeeId format is invalid."
}
```

**Pagination Object (`pagination`)**:
```json
{
  "page": 1,          // Current page number (1-indexed)
  "pageSize": 25,     // Number of items per page
  "totalCount": 100,  // Total number of items across all pages
  "totalPages": 4     // Total number of pages
}
```

---

## 5. Complete API Endpoint Specification

### 5.1 Authentication API (`/api/v1/auth`)

While primary authentication happens directly with Entra ID on the client side, the backend may need to exchange or validate tokens, or manage internal session state.

#### POST `/api/v1/auth/login`
*   **Description**: Validates the Entra ID JWT and establishes an internal session/profile if necessary.
*   **Method**: `POST`
*   **Roles**: `Any`
*   **Request Body**:
    ```json
    { "token": "eyJhbGci..." }
    ```
*   **Response (200 OK)**:
    ```json
    {
      "success": true,
      "data": { "userId": "123e4567-e89b-12d3-a456-426614174000", "roles": ["Manager"] },
      "message": "Login successful"
    }
    ```

#### POST `/api/v1/auth/refresh`
*   **Description**: Refreshes the internal session token (if applicable).
*   **Method**: `POST`

#### POST `/api/v1/auth/logout`
*   **Description**: Invalidates the current session on the backend.
*   **Method**: `POST`

#### GET `/api/v1/auth/me`
*   **Description**: Returns the profile, roles, and preferences of the currently authenticated user.
*   **Method**: `GET`
*   **Roles**: `Any`

---

### 5.2 Employee API (`/api/v1/employees`)

Manages access to employee directory information and specific employee history.

#### GET `/api/v1/employees`
*   **Description**: Lists employees based on filters.
*   **Query Params**: `department`, `office`, `managerId`, `status`, `page`, `pageSize`, `sortBy`, `sortDir`
*   **Roles**: `HR`, `Administrator`, `Manager` (restricted to subordinates)

#### GET `/api/v1/employees/{id}`
*   **Description**: Retrieves detailed profile of a specific employee.
*   **Roles**: `HR`, `Admin`, `Manager` (if subordinate), `Employee` (if self)
*   **Response (200 OK)**:
    ```json
    {
      "success": true,
      "data": {
        "id": "EMP-1001",
        "name": "Jane Doe",
        "email": "jane.doe@ramboll.in",
        "department": "Engineering",
        "officeLocation": "Ramboll Chennai",
        "managerId": "MGR-500",
        "managerName": "John Smith"
      }
    }
    ```

#### GET `/api/v1/employees/{id}/attendance`
*   **Description**: Retrieves historical daily attendance records for the employee.
*   **Query Params**: `startDate`, `endDate`, `page`, `pageSize`
*   **Roles**: `HR`, `Admin`, `Manager` (if subordinate), `Employee` (if self)

#### GET `/api/v1/employees/{id}/devices`
*   **Description**: Lists known devices (MAC/Hostnames) associated with the employee (sourced from Intune).
*   **Roles**: `HR`, `Admin`

#### GET `/api/v1/employees/{id}/history`
*   **Description**: Retrieves audit history of profile changes (e.g., manager change, department transfer).
*   **Roles**: `HR`, `Admin`

---

### 5.3 Attendance API (`/api/v1/attendance`)

Core API for accessing calculated presence data.

#### GET `/api/v1/attendance/daily`
*   **Description**: Retrieves a comprehensive list of daily attendance records across the organization.
*   **Query Params**: `date` (YYYY-MM-DD), `employeeId`, `department`, `office`, `type` (Present, Absent, Leave), `page`, `pageSize`
*   **Roles**: `HR`, `Admin`, `DepartmentHead`, `ExecutiveManagement`

#### GET `/api/v1/attendance/weekly`
*   **Description**: Retrieves summarized weekly compliance data (e.g., days in office out of 3).
*   **Query Params**: `weekStartDate`, `department`, `office`
*   **Roles**: `HR`, `Admin`, `Manager`

#### GET `/api/v1/attendance/monthly`
*   **Description**: Retrieves summarized monthly compliance and trends.
*   **Query Params**: `month` (YYYY-MM), `department`, `office`
*   **Roles**: `HR`, `Admin`, `Manager`

#### GET `/api/v1/attendance/summary`
*   **Description**: Provides high-level aggregate counts for a given date (e.g., Total Present, Total Absent per office).
*   **Query Params**: `date`
*   **Roles**: `HR`, `Admin`, `ExecutiveManagement`

#### GET `/api/v1/attendance/trends`
*   **Description**: Provides time-series data suitable for charting attendance trends over time.
*   **Query Params**: `startDate`, `endDate`, `groupBy` (day, week, month)

#### GET `/api/v1/attendance/employee/{id}/timeline/{date}`
*   **Description**: Retrieves the exact session start/stop times for a specific employee on a specific day. This shows exactly when they connected/disconnected from the office network.
*   **Roles**: `HR`, `Admin`, `Manager` (if subordinate), `Employee` (if self)
*   **Response (200 OK)**:
    ```json
    {
      "success": true,
      "data": {
        "date": "2023-10-25",
        "employeeId": "EMP-1001",
        "status": "Present",
        "totalDurationMinutes": 480,
        "sessions": [
          { "connectedAt": "2023-10-25T08:30:00Z", "disconnectedAt": "2023-10-25T12:00:00Z", "networkType": "Wi-Fi", "location": "Chennai Floor 3" },
          { "connectedAt": "2023-10-25T13:00:00Z", "disconnectedAt": "2023-10-25T17:30:00Z", "networkType": "LAN", "location": "Chennai Floor 3" }
        ]
      }
    }
    ```

---

### 5.4 Manager API (`/api/v1/manager`)

Endpoints explicitly scoped to the calling manager's hierarchy.

#### GET `/api/v1/manager/team`
*   **Description**: Returns all direct and indirect reports for the caller.
*   **Query Params**: `includeIndirect` (boolean)
*   **Roles**: `Manager`

#### GET `/api/v1/manager/team/attendance`
*   **Description**: Daily attendance view limited to the manager's team.
*   **Query Params**: `date`
*   **Roles**: `Manager`

#### GET `/api/v1/manager/team/compliance`
*   **Description**: Weekly compliance report for the manager's team highlighting non-compliant employees (< 3 days).
*   **Query Params**: `weekStartDate`
*   **Roles**: `Manager`

#### GET `/api/v1/manager/org-chart`
*   **Description**: Returns a hierarchical tree representation of the manager's reports.
*   **Roles**: `Manager`

#### GET `/api/v1/manager/team/weekly-summary`
*   **Description**: Aggregated statistics for the team's weekly performance.
*   **Roles**: `Manager`

#### POST `/api/v1/manager/trigger-weekly-report`
*   **Description**: Allows a manager to manually trigger the generation and email delivery of their team's weekly compliance report.
*   **Roles**: `Manager`

---

### 5.5 Department API (`/api/v1/departments`)

#### GET `/api/v1/departments`
*   **Description**: Lists all departments.
*   **Roles**: `Any`

#### GET `/api/v1/departments/{id}`
*   **Description**: Department details.
*   **Roles**: `Any`

#### GET `/api/v1/departments/{id}/attendance`
*   **Description**: Attendance aggregate for a specific department.
*   **Roles**: `DepartmentHead`, `HR`, `Admin`

#### GET `/api/v1/departments/{id}/employees`
*   **Description**: List employees in a department.
*   **Roles**: `DepartmentHead`, `HR`, `Admin`

#### GET `/api/v1/departments/{id}/compliance`
*   **Description**: Compliance metrics for the department.
*   **Roles**: `DepartmentHead`, `HR`, `Admin`

---

### 5.6 Reports API (`/api/v1/reports`)

Endpoints for generating and downloading structured reports.

*   `GET /api/v1/reports/daily`
*   `GET /api/v1/reports/weekly`
*   `GET /api/v1/reports/monthly`
*   `GET /api/v1/reports/department`
*   `GET /api/v1/reports/organization`
*   `GET /api/v1/reports/executive`

#### GET `/api/v1/reports/export`
*   **Description**: Triggers a data export.
*   **Query Params**: `reportType` (daily, weekly, etc.), `format` (CSV, Excel, PDF), `startDate`, `endDate`, `filters...`
*   **Response**: Returns the raw file bytes with appropriate `Content-Type` and `Content-Disposition` headers.

---

### 5.7 Admin API (`/api/v1/admin`)

Configuration and manual sync controls. Requires `Administrator` role.

#### Network & Location Configuration
*   `GET / POST / PUT / DELETE /api/v1/admin/office-networks`: Manage IP subnets, VLANs, and SSIDs that constitute "in-office".
*   `GET / POST / PUT / DELETE /api/v1/admin/office-locations`: Manage physical office locations (Chennai, Noida, etc.).

#### System Configuration
*   `GET / PUT /api/v1/admin/business-rules`: Configure global rules (e.g., minimum days in office = 3, session timeout = 30 mins).
*   `GET / PUT /api/v1/admin/email-templates`: Manage HTML templates for automated emails.
*   `GET / PUT /api/v1/admin/system-config`: Global settings (sync intervals, retention policies).

#### Synchronization Triggers
*   `POST /api/v1/admin/sync/users`: Force manual Entra ID user/hierarchy sync.
*   `POST /api/v1/admin/sync/devices`: Force manual Intune device sync.
*   `POST /api/v1/admin/sync/telemetry`: Force immediate processing of pending network telemetry logs.

#### Auditing
*   `GET /api/v1/admin/audit-logs`: System-wide audit logs (who changed what configuration).
*   `GET /api/v1/admin/api-logs`: Application logs for debugging API failures.

---

### 5.8 Dashboard API (`/api/v1/dashboard`)

Bespoke endpoints that aggregate multiple data points into a single payload to populate specific UI dashboards efficiently, reducing round-trips.

*   `GET /api/v1/dashboard/employee`: Returns employee's own compliance status, recent attendance, and next expected office days.
*   `GET /api/v1/dashboard/manager`: Returns team compliance pie charts, top absentees, and weekly summary.
*   `GET /api/v1/dashboard/hr`: Organization-wide compliance stats, office utilization heatmaps.
*   `GET /api/v1/dashboard/executive`: High-level metrics, cross-department comparisons.
*   `GET /api/v1/dashboard/admin`: System health overview, last sync times, error rates.

---

### 5.9 Health API (`/health`)

Standard endpoints for Kubernetes/Load Balancer health checks. (These do not require authentication or follow the base response model to minimize overhead).

*   `GET /health`: Basic overall health status. Returns `200 OK` or `503 Service Unavailable`.
*   `GET /health/ready`: Checks if dependencies (DB, Entra ID, Cache) are reachable. Returns `200 OK` if ready to serve traffic.
*   `GET /health/live`: Checks if the API process is running. Returns `200 OK`.

---

## 6. Rate Limiting Configuration

To protect the platform from abuse and ensure fair resource allocation, rate limiting is implemented at the API Gateway / Middleware level.

| Scope | Limit | Window | Action on Exceed |
| :--- | :--- | :--- | :--- |
| Standard Endpoints (GETs) | 100 requests | per 1 minute per User (IP/Token) | Return `429 Too Many Requests` |
| Reporting / Export Endpoints | 10 requests | per 1 minute per User | Return `429 Too Many Requests` |
| Admin Sync Triggers | 2 requests | per 5 minutes per Admin | Return `429 Too Many Requests` |
| Global API Limit | 5000 requests | per 1 minute per IP Address | Reject connection |

Response headers included on rate-limited requests:
*   `X-RateLimit-Limit`: Maximum requests allowed.
*   `X-RateLimit-Remaining`: Remaining requests in current window.
*   `X-RateLimit-Reset`: UTC epoch time when the rate limit resets.
*   `Retry-After`: Seconds to wait before retrying.

---

## 7. API Versioning Strategy

The API employs **URI Versioning**. The version is included in the base path of the URL.
Example: `/api/v1/employees`

**Policies**:
*   **Current Version**: `v1`. All new development will target this version.
*   **Breaking Changes**: Any change that alters a request schema, removes a field from a response, changes standard behavior, or alters authorization requirements is considered breaking and will necessitate a new version (e.g., `v2`).
*   **Non-Breaking Changes**: Adding new endpoints, adding new optional request parameters, or adding new fields to a response payload will be applied to the current version.
*   **Deprecation**: When a new version is introduced, the old version will be marked as deprecated via a custom header (`X-API-Deprecated: true`) and a sunset date will be communicated. The old API will remain operational for a minimum of 6 months.

---

## 8. Swagger/OpenAPI Configuration

The platform utilizes Swashbuckle (ASP.NET Core) to automatically generate OpenAPI 3.0 specifications.

*   **Endpoint**: `/swagger/v1/swagger.json`
*   **UI Access**: `/swagger` (Available only in `Development` and `Staging` environments; disabled in `Production` for security).
*   **Configuration**:
    *   JWT Bearer Authentication is configured in Swagger UI, allowing developers to paste their token to test endpoints directly from the browser.
    *   XML Comments in C# code are strictly enforced to populate endpoint descriptions, parameter details, and response models in the OpenAPI spec.

---

## 9. Error Handling & Status Code Reference Table

The API uses standard HTTP status codes to indicate the outcome of a request. When an error occurs, the standardized Base Response Model includes an `errors` array providing actionable details.

| HTTP Status | Name | Description & Usage |
| :--- | :--- | :--- |
| `200` | OK | Request succeeded. Response body contains requested data. |
| `201` | Created | Resource was successfully created (e.g., Admin creates a network rule). |
| `204` | No Content | Request succeeded, but no data to return (e.g., successful DELETE). |
| `400` | Bad Request | The client sent invalid input (e.g., malformed JSON, validation failure). The `errors` array will detail specific field validation failures. |
| `401` | Unauthorized | The request lacks valid authentication credentials (missing, expired, or invalid JWT). |
| `403` | Forbidden | The client is authenticated but lacks permission to perform the action or access the resource (e.g., Manager trying to access a non-report's data). |
| `404` | Not Found | The requested resource (URL or specific entity ID) does not exist. |
| `405` | Method Not Allowed | The endpoint exists, but the HTTP method used is not supported. |
| `409` | Conflict | The request conflicts with the current state of the server (e.g., creating a duplicate rule). |
| `429` | Too Many Requests | The client has exceeded the configured rate limit. |
| `500` | Internal Server Error | An unexpected condition was encountered. A correlation ID is provided in the `message` for IT support tracking. |
| `503` | Service Unavailable | The server is temporarily unable to handle the request (e.g., backend database down). |

---

## 10. Pagination, Filtering, Sorting Conventions

To ensure consistent behavior across endpoints that return collections, standard query parameters are enforced.

### Pagination
*   `page`: (Integer) The page number to retrieve. Defaults to 1. Minimum 1.
*   `pageSize`: (Integer) Number of records per page. Defaults to 25. Maximum 100.
*   **Example**: `GET /api/v1/employees?page=2&pageSize=50`

### Filtering
*   Filters are passed as query string parameters matching field names.
*   Arrays can be passed by repeating the parameter or comma-separating (e.g., `office=Chennai&office=Noida` or `office=Chennai,Noida`).
*   Dates should always be in ISO 8601 format (`YYYY-MM-DD` or `YYYY-MM-DDThh:mm:ssZ`).

### Sorting
*   `sortBy`: (String) The field name to sort by (e.g., `name`, `date`).
*   `sortDir`: (String) The direction to sort. Accepts `asc` or `desc`. Defaults to `asc`.
*   **Example**: `GET /api/v1/attendance/daily?sortBy=date&sortDir=desc`

---

## 11. Edge Cases, Assumptions, Risks

### Edge Cases Handled via API Design
*   **Massive Hierarchies**: `GET /api/v1/manager/team` defaults to `includeIndirect=false` to prevent massive payload responses for high-level executives. Executives should rely on aggregate endpoints (`/api/v1/dashboard/executive`) rather than pulling thousands of raw employee records.
*   **Timezone Discrepancies**: All dates and times are stored and returned in UTC (`Z`). The frontend is responsible for localizing timestamps to IST (Indian Standard Time, UTC+5:30) for presentation.

### Assumptions
*   **Entra ID Integrity**: The system assumes the organizational hierarchy in Entra ID is accurate and regularly maintained. Incorrect Entra ID data will result in Managers seeing incorrect data via the API.
*   **Clock Sync**: Assumes all network infrastructure (switches, controllers) sending telemetry data is synchronized to accurate NTP servers to ensure timestamp correlation.

### Risks & Mitigations
*   **Risk**: Complex Manager API queries (e.g., calculating weekly compliance dynamically for 1000 indirect reports) could cause database strain and API timeouts.
    *   **Mitigation**: The backend will utilize CQRS and pre-calculated materialized views for aggregated data. The API will query flat summary tables rather than performing complex hierarchical JOINs and aggregations on the fly.
*   **Risk**: Stale Token Usage.
    *   **Mitigation**: The API strictly validates token expiry. For immediate revocation (e.g., termination), the API Gateway maintains a brief blacklist cache synchronized with Entra ID's continuous access evaluation (CAE).
*   **Risk**: Data Exfiltration via Export APIs.
    *   **Mitigation**: Strict RBAC on `/api/v1/reports/export`. Extensive auditing (logging `userId`, `IP`, `timestamp`, `reportType`, `parameters`) for every export request. Rate limiting applied specifically to export endpoints.
