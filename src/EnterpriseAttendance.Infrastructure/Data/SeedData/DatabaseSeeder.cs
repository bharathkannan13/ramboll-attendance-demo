using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EnterpriseAttendance.Core.Entities;
using EnterpriseAttendance.Core.Enums;

namespace EnterpriseAttendance.Infrastructure.Data.SeedData
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AttendanceDbContext context)
        {
            if (await context.OfficeLocations.AnyAsync()) return;

            // 1. Seed Indian Office Locations
            var offices = new List<OfficeLocation>
            {
                new OfficeLocation { Name = "Bkran Group Chennai Campus", City = "Chennai", State = "Tamil Nadu", Country = "India", TimeZone = "India Standard Time" },
                new OfficeLocation { Name = "Bkran Group Noida Tech Park", City = "Noida", State = "Uttar Pradesh", Country = "India", TimeZone = "India Standard Time" },
                new OfficeLocation { Name = "Bkran Group Hyderabad Hub", City = "Hyderabad", State = "Telangana", Country = "India", TimeZone = "India Standard Time" },
                new OfficeLocation { Name = "Bkran Group Gurugram CyberCity", City = "Gurugram", State = "Haryana", Country = "India", TimeZone = "India Standard Time" },
                new OfficeLocation { Name = "Bkran Group Bangalore Innovation Center", City = "Bangalore", State = "Karnataka", Country = "India", TimeZone = "India Standard Time" }
            };

            await context.OfficeLocations.AddRangeAsync(offices);
            await context.SaveChangesAsync();

            // 2. Seed Corporate Office Networks
            var networks = new List<OfficeNetwork>
            {
                new OfficeNetwork { OfficeLocationId = offices[0].Id, NetworkType = NetworkType.SSID, NetworkValue = "Bkran-CHN-Corporate", Description = "Chennai Office Corporate Wi-Fi" },
                new OfficeNetwork { OfficeLocationId = offices[0].Id, NetworkType = NetworkType.Subnet, NetworkValue = "10.100.*", Description = "Chennai Corporate Subnet" },
                new OfficeNetwork { OfficeLocationId = offices[1].Id, NetworkType = NetworkType.SSID, NetworkValue = "Bkran-NOI-Corporate", Description = "Noida Office Corporate Wi-Fi" },
                new OfficeNetwork { OfficeLocationId = offices[1].Id, NetworkType = NetworkType.Subnet, NetworkValue = "10.101.*", Description = "Noida Corporate Subnet" },
                new OfficeNetwork { OfficeLocationId = offices[2].Id, NetworkType = NetworkType.SSID, NetworkValue = "Bkran-HYD-Corporate", Description = "Hyderabad Office Corporate Wi-Fi" },
                new OfficeNetwork { OfficeLocationId = offices[2].Id, NetworkType = NetworkType.Subnet, NetworkValue = "10.102.*", Description = "Hyderabad Corporate Subnet" }
            };

            await context.OfficeNetworks.AddRangeAsync(networks);
            await context.SaveChangesAsync();

            // 3. Seed Departments
            var departments = new List<Department>
            {
                new Department { Name = "Executive Leadership", Code = "EXEC" },
                new Department { Name = "Software Engineering", Code = "ENG" },
                new Department { Name = "IT Infrastructure & Security", Code = "IT" },
                new Department { Name = "Human Resources", Code = "HR" },
                new Department { Name = "Sales & Business Development", Code = "SALES" }
            };

            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();

            // 4. Seed Business Rules & Config
            var rules = new List<BusinessRule>
            {
                new BusinessRule { RuleName = "Grace Period Minutes", RuleKey = "GracePeriodMinutes", RuleValue = "30", DataType = "System.Int32", Description = "Max gap before starting new session" },
                new BusinessRule { RuleName = "Target In-Office Days Per Week", RuleKey = "TargetOfficeDaysPerWeek", RuleValue = "3", DataType = "System.Int32", Description = "Mandatory in-office days per week" }
            };

            await context.BusinessRules.AddRangeAsync(rules);

            var configs = new List<SystemConfiguration>
            {
                new SystemConfiguration { ConfigKey = "TelemetrySettings:UseMockTelemetry", ConfigValue = "true", Description = "Set to true for Standalone Demo, false for Live M365 API" },
                new SystemConfiguration { ConfigKey = "AzureAd:TenantId", ConfigValue = "<< PASTE YOUR AZURE TENANT ID HERE >>", Description = "Microsoft Entra ID Tenant ID" },
                new SystemConfiguration { ConfigKey = "AzureAd:ClientId", ConfigValue = "<< PASTE YOUR APP REGISTRATION CLIENT ID HERE >>", Description = "Microsoft Azure App Registration Client ID" },
                new SystemConfiguration { ConfigKey = "AzureAd:ClientSecret", ConfigValue = "<< PASTE YOUR CLIENT SECRET VALUE HERE >>", Description = "Microsoft Azure App Registration Client Secret", IsEncrypted = true }
            };

            await context.SystemConfigurations.AddRangeAsync(configs);
            await context.SaveChangesAsync();

            // 5. Seed Multi-Level Hierarchy
            // Level 1: CEO
            var ceo = new Employee
            {
                EntraObjectId = "00000000-0000-0000-0000-000000000001",
                Email = "rajesh.sharma@bkrangroup.com",
                FullName = "Rajesh Sharma",
                Title = "Managing Director & CEO",
                EmployeeCode = "BK-IND-001",
                DepartmentId = departments[0].Id,
                OfficeLocationId = offices[0].Id,
                Role = UserRole.Administrator
            };
            await context.Employees.AddAsync(ceo);
            await context.SaveChangesAsync();

            // Level 2: VP Engineering & VP Sales
            var vpEng = new Employee
            {
                EntraObjectId = "00000000-0000-0000-0000-000000000002",
                Email = "sarah.connor@bkrangroup.com",
                FullName = "Sarah Connor",
                Title = "VP of Engineering",
                EmployeeCode = "BK-IND-002",
                DepartmentId = departments[1].Id,
                ManagerId = ceo.Id,
                OfficeLocationId = offices[0].Id,
                Role = UserRole.Manager
            };

            var vpSales = new Employee
            {
                EntraObjectId = "00000000-0000-0000-0000-000000000003",
                Email = "amit.patel@bkrangroup.com",
                FullName = "Amit Patel",
                Title = "VP of Sales",
                EmployeeCode = "BK-IND-003",
                DepartmentId = departments[4].Id,
                ManagerId = ceo.Id,
                OfficeLocationId = offices[1].Id,
                Role = UserRole.Manager
            };

            await context.Employees.AddRangeAsync(vpEng, vpSales);
            await context.SaveChangesAsync();

            // Level 3: Managers (including Bharath Kannan & Kwame Mensah)
            var mgrBharath = new Employee
            {
                EntraObjectId = "00000000-0000-0000-0000-000000000099",
                Email = "bharathkannan1154@gmail.com",
                FullName = "Bharath Kannan",
                Title = "Engineering Manager - Chennai Campus",
                EmployeeCode = "BK-IND-099",
                DepartmentId = departments[1].Id,
                ManagerId = vpEng.Id,
                OfficeLocationId = offices[0].Id,
                Role = UserRole.Manager
            };

            var mgrEngChennai = new Employee
            {
                EntraObjectId = "00000000-0000-0000-0000-000000000004",
                Email = "kwame.mensah@bkrangroup.com",
                FullName = "Kwame Mensah",
                Title = "Engineering Manager - Chennai",
                EmployeeCode = "BK-IND-004",
                DepartmentId = departments[1].Id,
                ManagerId = vpEng.Id,
                OfficeLocationId = offices[0].Id,
                Role = UserRole.Manager
            };

            var mgrEngNoida = new Employee
            {
                EntraObjectId = "00000000-0000-0000-0000-000000000005",
                Email = "priya.verma@bkrangroup.com",
                FullName = "Priya Verma",
                Title = "Engineering Manager - Noida",
                EmployeeCode = "BK-IND-005",
                DepartmentId = departments[1].Id,
                ManagerId = vpEng.Id,
                OfficeLocationId = offices[1].Id,
                Role = UserRole.Manager
            };

            await context.Employees.AddRangeAsync(mgrBharath, mgrEngChennai, mgrEngNoida);
            await context.SaveChangesAsync();

            // Level 4: Direct reports reporting to Bharath Kannan & Kwame Mensah
            var anand = new Employee { EntraObjectId = Guid.NewGuid().ToString(), Email = "anand.kumar@bkrangroup.com", FullName = "Anand Kumar", Title = "Senior Software Engineer", EmployeeCode = "BK-IND-007", DepartmentId = departments[1].Id, ManagerId = mgrBharath.Id, OfficeLocationId = offices[0].Id, Role = UserRole.Manager };
            var deepa = new Employee { EntraObjectId = Guid.NewGuid().ToString(), Email = "deepa.srinivasan@bkrangroup.com", FullName = "Deepa Srinivasan", Title = "Full Stack Developer", EmployeeCode = "BK-IND-008", DepartmentId = departments[1].Id, ManagerId = mgrBharath.Id, OfficeLocationId = offices[0].Id, Role = UserRole.Manager };
            var karthik = new Employee { EntraObjectId = Guid.NewGuid().ToString(), Email = "karthik.rajan@bkrangroup.com", FullName = "Karthik Rajan", Title = "Cloud Lead Architect", EmployeeCode = "BK-IND-009", DepartmentId = departments[1].Id, ManagerId = mgrBharath.Id, OfficeLocationId = offices[0].Id, Role = UserRole.Manager };
            var vikram = new Employee { EntraObjectId = Guid.NewGuid().ToString(), Email = "vikram.seth@bkrangroup.com", FullName = "Vikram Seth", Title = "Frontend Engineer", EmployeeCode = "BK-IND-010", DepartmentId = departments[1].Id, ManagerId = mgrBharath.Id, OfficeLocationId = offices[0].Id, Role = UserRole.Manager };
            var suresh = new Employee { EntraObjectId = Guid.NewGuid().ToString(), Email = "suresh.raina@bkrangroup.com", FullName = "Suresh Raina", Title = "DevOps Specialist", EmployeeCode = "BK-IND-011", DepartmentId = departments[1].Id, ManagerId = mgrBharath.Id, OfficeLocationId = offices[0].Id, Role = UserRole.Manager };

            await context.Employees.AddRangeAsync(anand, deepa, karthik, vikram, suresh);
            await context.SaveChangesAsync();

            // Level 5: Sub-reporting employees reporting to Karthik Rajan
            var subKarthik1 = new Employee { EntraObjectId = Guid.NewGuid().ToString(), Email = "rahul.dravid@bkrangroup.com", FullName = "Rahul Dravid", Title = "Cloud DevOps Engineer", EmployeeCode = "BK-IND-014", DepartmentId = departments[1].Id, ManagerId = karthik.Id, OfficeLocationId = offices[0].Id, Role = UserRole.Manager };
            var subKarthik2 = new Employee { EntraObjectId = Guid.NewGuid().ToString(), Email = "meera.nair@bkrangroup.com", FullName = "Meera Nair", Title = "Cloud Security Analyst", EmployeeCode = "BK-IND-015", DepartmentId = departments[1].Id, ManagerId = karthik.Id, OfficeLocationId = offices[0].Id, Role = UserRole.Manager };

            await context.Employees.AddRangeAsync(subKarthik1, subKarthik2);
            await context.SaveChangesAsync();

            // Seed Intune Devices
            var allEmps = await context.Employees.ToListAsync();
            var devices = new List<Device>();

            foreach (var emp in allEmps)
            {
                devices.Add(new Device
                {
                    IntuneDeviceId = Guid.NewGuid().ToString(),
                    DefenderDeviceId = Guid.NewGuid().ToString(),
                    DeviceName = $"BK-LAP-{emp.EmployeeCode.Replace("BK-IND-", "")}",
                    SerialNumber = $"SN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    EmployeeId = emp.Id,
                    OperatingSystem = "Windows 11 Enterprise",
                    ComplianceStatus = ComplianceStatus.Compliant,
                    IsManaged = true
                });
            }

            await context.Devices.AddRangeAsync(devices);
            await context.SaveChangesAsync();

            // Seed attendance history for all employees
            var today = DateTime.Today;
            var random = new Random();

            foreach (var emp in allEmps)
            {
                var empDevice = devices.FirstOrDefault(d => d.EmployeeId == emp.Id);

                for (int i = 14; i >= 0; i--)
                {
                    var date = today.AddDays(-i);
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) continue;

                    bool isInOffice = random.Next(100) < 75;
                    var officeLoc = offices.FirstOrDefault(o => o.Id == (emp.OfficeLocationId ?? offices[0].Id));

                    var firstSeen = date.AddHours(9).AddMinutes(random.Next(0, 30));
                    var lastSeen = date.AddHours(17).AddMinutes(random.Next(30, 60));
                    double hours = (lastSeen - firstSeen).TotalHours - 0.75;

                    var daily = new DailyAttendance
                    {
                        EmployeeId = emp.Id,
                        AttendanceDate = date,
                        OfficeLocationId = isInOffice ? officeLoc?.Id : null,
                        AttendanceType = isInOffice ? AttendanceType.Office : AttendanceType.WFH,
                        FirstSeenTime = firstSeen,
                        LastSeenTime = lastSeen,
                        TotalOfficeHours = isInOffice ? Math.Round(hours, 2) : 0.0,
                        TotalSessions = 1,
                        PrimaryNetworkType = isInOffice ? NetworkLocationType.CorporateOffice : NetworkLocationType.Remote,
                        IsHybridCompliant = isInOffice
                    };
                    await context.DailyAttendances.AddAsync(daily);
                }
            }

            await context.SaveChangesAsync();

            // =====================================================================
            // SEED ALL 18 ENTERPRISE SPECIFICATION TABLES
            // =====================================================================

            // 1. Application_Ownership
            if (!await context.ApplicationOwnerships.AnyAsync())
            {
                await context.ApplicationOwnerships.AddAsync(new ApplicationOwnership
                {
                    Application_Name = "Bkran Group Connect Attendance Portal",
                    Business_Owner = "Human Resources (HR)",
                    Technical_Owner = "Cyber Security & Infrastructure",
                    Support_Group = "Global Enterprise IT Support",
                    Escalation_Email = "escalations-attendance@bkrangroup.com"
                });
            }

            // 2. Role_Master
            if (!await context.RoleMasters.AnyAsync())
            {
                await context.RoleMasters.AddRangeAsync(
                    new RoleMaster { Role_Name = "Admin", Description = "Full Enterprise System Administrator Access" },
                    new RoleMaster { Role_Name = "HR", Description = "Human Resources Global Attendance & Policy Manager" },
                    new RoleMaster { Role_Name = "Manager", Description = "People Manager Reporting Tree Access" },
                    new RoleMaster { Role_Name = "Employee", Description = "Individual Contributor Self-Service Access" },
                    new RoleMaster { Role_Name = "Security Analyst", Description = "Cybersecurity Threat & Anomaly Monitoring" }
                );
            }

            // 3. Office_Master (Support Chennai, Bangalore, Mumbai, Pune, Delhi, Noida, Hyderabad, Gurugram)
            if (!await context.OfficeMasters.AnyAsync())
            {
                await context.OfficeMasters.AddRangeAsync(
                    new OfficeMaster { Office_Name = "Chennai Campus", City = "Chennai", Country = "India", Timezone = "IST" },
                    new OfficeMaster { Office_Name = "Bangalore Innovation Center", City = "Bangalore", Country = "India", Timezone = "IST" },
                    new OfficeMaster { Office_Name = "Mumbai Corporate Tower", City = "Mumbai", Country = "India", Timezone = "IST" },
                    new OfficeMaster { Office_Name = "Pune Tech Hub", City = "Pune", Country = "India", Timezone = "IST" },
                    new OfficeMaster { Office_Name = "Delhi Executive Office", City = "Delhi", Country = "India", Timezone = "IST" },
                    new OfficeMaster { Office_Name = "Noida Tech Park", City = "Noida", Country = "India", Timezone = "IST" },
                    new OfficeMaster { Office_Name = "Hyderabad Hub", City = "Hyderabad", Country = "India", Timezone = "IST" },
                    new OfficeMaster { Office_Name = "Gurugram CyberCity", City = "Gurugram", Country = "India", Timezone = "IST" }
                );
            }

            // 4. Work_Mode_Master (Office, WFH, Client Site, Travel)
            if (!await context.WorkModeMasters.AnyAsync())
            {
                await context.WorkModeMasters.AddRangeAsync(
                    new WorkModeMaster { Mode_Name = "Office", Description = "Physical presence at corporate branch office" },
                    new WorkModeMaster { Mode_Name = "WFH", Description = "Authorized Remote / Work From Home network" },
                    new WorkModeMaster { Mode_Name = "Client Site", Description = "Authorized client site location deployment" },
                    new WorkModeMaster { Mode_Name = "Travel", Description = "Official corporate business travel" }
                );
            }

            // 5. Retention_Config (Attendance=2555 days, Audit=3650 days, Error Logs=365 days)
            if (!await context.RetentionConfigs.AnyAsync())
            {
                await context.RetentionConfigs.AddRangeAsync(
                    new RetentionConfig { Table_Name = "Attendance_Log", Retention_Days = 2555, Archive_Flag = true },
                    new RetentionConfig { Table_Name = "Security_Audit_Log", Retention_Days = 3650, Archive_Flag = true },
                    new RetentionConfig { Table_Name = "Error_Log", Retention_Days = 365, Archive_Flag = false }
                );
            }

            // 6. Backup_Log
            if (!await context.BackupLogs.AnyAsync())
            {
                await context.BackupLogs.AddRangeAsync(
                    new BackupLog { Backup_Type = "Daily", Backup_Status = "COMPLETED", Backup_Location = "Azure Primary Storage Vault" },
                    new BackupLog { Backup_Type = "Weekly", Backup_Status = "COMPLETED", Backup_Location = "Azure Secondary Geo-Redundant Storage" }
                );
            }

            // 7. Integration_Config (Entra ID, Workday, Cisco ISE, Intune, Active Directory)
            if (!await context.IntegrationConfigs.AnyAsync())
            {
                await context.IntegrationConfigs.AddRangeAsync(
                    new IntegrationConfig { System_Name = "Microsoft Entra ID", Endpoint_URL = "https://graph.microsoft.com/v1.0/users", Status = "CONNECTED" },
                    new IntegrationConfig { System_Name = "Microsoft Intune", Endpoint_URL = "https://graph.microsoft.com/v1.0/deviceManagement", Status = "CONNECTED" },
                    new IntegrationConfig { System_Name = "Cisco ISE", Endpoint_URL = "https://ise.bkrangroup.com/api/v1/sessions", Status = "ACTIVE" },
                    new IntegrationConfig { System_Name = "Workday HCM", Endpoint_URL = "https://wd3.workday.com/bkrangroup/api", Status = "SYNCED" },
                    new IntegrationConfig { System_Name = "Active Directory", Endpoint_URL = "ldaps://ad.bkrangroup.com:636", Status = "ACTIVE" }
                );
            }

            // 8. Security_Audit_Log
            if (!await context.SecurityAuditLogs.AnyAsync())
            {
                await context.SecurityAuditLogs.AddRangeAsync(
                    new SecurityAuditLog { Employee_ID = 1, Employee_Name = "Rajesh Sharma", Action_Type = "Login", IP_Address = "10.100.12.45", Device_Name = "BK-LAP-001", Result = "SUCCESS", Remarks = "SSO Authentication Passed" },
                    new SecurityAuditLog { Employee_ID = 4, Employee_Name = "Bharath Kannan", Action_Type = "Report Export", IP_Address = "10.100.14.88", Device_Name = "BK-LAP-099", Result = "SUCCESS", Remarks = "Exported Weekly Excel Summary" }
                );
            }

            // 9. Login_Session_Log
            if (!await context.LoginSessionLogs.AnyAsync())
            {
                await context.LoginSessionLogs.AddAsync(new LoginSessionLog
                {
                    Employee_ID = 4,
                    IP_Address = "10.100.14.88",
                    Browser = "Chrome 122.0 / Windows 11",
                    Device_Name = "BK-LAP-099",
                    Session_Status = "ACTIVE"
                });
            }

            // 10. Error_Log
            if (!await context.ErrorLogs.AnyAsync())
            {
                await context.ErrorLogs.AddAsync(new ErrorLog
                {
                    Error_Source = "TelemetryEngine",
                    Error_Message = "Minor transient network jitter resolved automatically",
                    Stack_Trace = "None",
                    Severity = "Low",
                    Status = "RESOLVED"
                });
            }

            // 11. Attendance_Risk_Log (Cybersecurity Threat Features)
            if (!await context.AttendanceRiskLogs.AnyAsync())
            {
                await context.AttendanceRiskLogs.AddRangeAsync(
                    new AttendanceRiskLog { Employee_ID = 5, Employee_Name = "Anand Kumar", Risk_Type = "Multiple Devices", Risk_Score = 35, Status = "RESOLVED" },
                    new AttendanceRiskLog { Employee_ID = 8, Employee_Name = "Vikram Seth", Risk_Type = "Suspicious Network Pattern", Risk_Score = 40, Status = "INVESTIGATING" }
                );
            }

            // 12. Analytics_Log (AI Predictions)
            if (!await context.AnalyticsLogs.AnyAsync())
            {
                await context.AnalyticsLogs.AddRangeAsync(
                    new AnalyticsLog { Prediction_Type = "Attendance Trend", Prediction_Value = "Projected 89.2% Office Occupancy for Next Week" },
                    new AnalyticsLog { Prediction_Type = "Peak Login Hours", Prediction_Value = "Peak Network Arrival Window: 09:15 AM - 09:45 AM IST" }
                );
            }

            await context.SaveChangesAsync();
        }
    }
}
