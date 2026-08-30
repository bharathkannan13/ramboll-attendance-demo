# Ramboll Enterprise Non-Tech Deployment & Operations Playbook

> **Document ID**: PLAYBOOK-2026-NONTECH-001  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Scope**: India Regional Offices (**Chennai, Noida, Hyderabad, Gurugram, Bangalore, Mumbai, Pune, Delhi**)

---

## 📌 Phase 1: Software Setup on Your Official Office Laptop

```powershell
# 1. Download approved source code
git clone https://github.com/bharathkannan13/ramboll-attendance-demo.git

# 2. Move into project folder
cd ramboll-attendance-demo/EnterpriseAttendance

# 3. Run application locally
dotnet run --project src/EnterpriseAttendance.Web/EnterpriseAttendance.Web.csproj
```

---

## ☁️ Phase 2: METHOD 1 — Azure App Service + Azure SQL Database (Recommended Setup)

### Step-by-Step Azure Portal Actions (`portal.azure.com`):

1. **Create Azure SQL Database**:
   - Search `SQL databases` &rarr; Click **+ Create**.
   - Server name: `sqlserver-ramboll-india` | Admin: `rambolladmin` | Password: `CreateASecurePassword123!`.
   - Copy ADO.NET Connection String from *Connection strings* tab.

2. **Create Azure App Service**:
   - Search `App Services` &rarr; Click **+ Create** &rarr; Select **Web App**.
   - Runtime stack: **.NET 8 (LTS)** | OS: *Linux/Windows* | Region: *South India*.
   - Name: `ramboll-attendance-portal` (`https://ramboll-attendance-portal.azurewebsites.net`).

3. **Connect GitHub CI/CD**:
   - Go to *Deployment Center* &rarr; Select **GitHub** &rarr; Select repo `ramboll-attendance-demo` &rarr; Branch `main` &rarr; **Save**.

4. **Set Environment Variables**:
   - Go to *Environment variables* &rarr; Add keys:
     - `TelemetrySettings__UseMockTelemetry` = `false`
     - `AzureAd__TenantId` = `[YOUR_AZURE_TENANT_ID]`
     - `AzureAd__ClientId` = `[YOUR_AZURE_CLIENT_ID]`
     - `AzureAd__ClientSecret` = `[YOUR_CLIENT_SECRET_VALUE]`
     - `ConnectionStrings__DefaultConnection` = `[YOUR_AZURE_SQL_CONNECTION_STRING]`

5. **Done!** Public Domain: **`https://ramboll-attendance-portal.azurewebsites.net`**

---

## 🏢 Phase 3: METHOD 2 — On-Premise IIS + Azure AD App Proxy (Hybrid Setup)

1. **Publish Binaries**: `dotnet publish src/EnterpriseAttendance.Web/EnterpriseAttendance.Web.csproj -c Release -o C:\inetpub\wwwroot\BkranAttendance`
2. **Bind IIS Site**: Create site `BkranAttendance` bound to `http://attendance.ramboll.local`.
3. **Configure Azure AD App Proxy**: In Entra ID Portal, download Connector to server and map Internal URL `http://attendance.ramboll.local/` to External URL `https://attendance.ramboll.com`.
