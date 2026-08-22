using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;
using EnterpriseAttendance.Core.Entities;
using EnterpriseAttendance.Core.Enums;
using EnterpriseAttendance.Infrastructure.Data;
using EnterpriseAttendance.Services.Engine;

namespace EnterpriseAttendance.Tests.EngineTests
{
    public class AttendanceEngineTests
    {
        private AttendanceDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AttendanceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AttendanceDbContext(options);
        }

        [Fact]
        public async Task ClassifyNetwork_CorporateSSID_ClassifiesAsCorporateOffice()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var office = new OfficeLocation { Name = "Bkran Group Chennai Campus", City = "Chennai" };
            await context.OfficeLocations.AddAsync(office);
            await context.SaveChangesAsync();

            var network = new OfficeNetwork
            {
                OfficeLocationId = office.Id,
                NetworkType = NetworkType.SSID,
                NetworkValue = "Bkran-CHN-Corporate"
            };
            await context.OfficeNetworks.AddAsync(network);
            await context.SaveChangesAsync();

            var classifier = new NetworkClassifier(context);

            // Act
            var result = await classifier.ClassifyNetworkAsync("10.100.45.10", "Bkran-CHN-Corporate", "10.100.0.0/16");

            // Assert
            result.LocationType.Should().Be(NetworkLocationType.CorporateOffice);
            result.OfficeLocationId.Should().Be(office.Id);
        }

        [Fact]
        public async Task ClassifyNetwork_HomeWiFi_ClassifiesAsRemote()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var classifier = new NetworkClassifier(context);

            // Act
            var result = await classifier.ClassifyNetworkAsync("192.168.1.15", "Home-WiFi-5G", "");

            // Assert
            result.LocationType.Should().Be(NetworkLocationType.Remote);
            result.OfficeLocationId.Should().BeNull();
        }

        [Fact]
        public async Task GracePeriod_Within30Mins_ExtendsExistingSession()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var emp = new Employee { Email = "test@bkrangroup.com", FullName = "Test Employee" };
            await context.Employees.AddAsync(emp);
            await context.SaveChangesAsync();

            var sessionManager = new SessionManager(context);
            var now = DateTime.Now;

            // Session 1: 09:00 AM
            await sessionManager.CreateOrUpdateSessionAsync(emp, null, now, NetworkLocationType.CorporateOffice, 1, 1, "10.100.1.1", "Bkran-CHN-Corporate");

            // Event 2: 09:15 AM (15 mins gap < 30 min grace period)
            var session2 = await sessionManager.CreateOrUpdateSessionAsync(emp, null, now.AddMinutes(15), NetworkLocationType.CorporateOffice, 1, 1, "10.100.1.1", "Bkran-CHN-Corporate");

            // Assert
            session2.DurationMinutes.Should().Be(15);
            session2.SessionStatus.Should().Be(SessionStatus.Active);
        }

        [Fact]
        public async Task NonCompliantDevice_TelemetryRejected_AuditedInLogs()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var emp = new Employee { Email = "unmanaged@bkrangroup.com", FullName = "Unmanaged Employee" };
            await context.Employees.AddAsync(emp);
            await context.SaveChangesAsync();

            var badDevice = new Device
            {
                IntuneDeviceId = Guid.NewGuid().ToString(),
                DeviceName = "UNAPPROVED-BYOD-LAPTOP",
                EmployeeId = emp.Id,
                ComplianceStatus = ComplianceStatus.NonCompliant,
                IsManaged = false
            };
            await context.Devices.AddAsync(badDevice);
            await context.SaveChangesAsync();

            var classifier = new NetworkClassifier(context);
            var sessionManager = new SessionManager(context);
            var engine = new AttendanceEngine(context, classifier, sessionManager);

            var evt = new TelemetryEvent
            {
                EmployeeId = emp.Id,
                DeviceId = badDevice.Id,
                TelemetrySource = TelemetrySource.Defender,
                EventType = EventType.Logon,
                Timestamp = DateTime.Now,
                IPAddress = "10.100.1.50",
                NetworkSSID = "Bkran-CHN-Corporate"
            };
            await context.TelemetryEvents.AddAsync(evt);
            await context.SaveChangesAsync();

            // Act
            await engine.ProcessTelemetryEventAsync(evt);

            // Assert
            var audit = await context.AuditLogs.FirstOrDefaultAsync(a => a.Action == "Telemetry_Rejected_NonCompliant_Device");
            audit.Should().NotBeNull();
            audit!.UserEmail.Should().Be("unmanaged@bkrangroup.com");
        }
    }
}
