using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EnterpriseAttendance.Core.Entities;
using EnterpriseAttendance.Core.Enums;
using EnterpriseAttendance.Core.Interfaces;
using EnterpriseAttendance.Infrastructure.Data;

namespace EnterpriseAttendance.Infrastructure.ExternalServices.Mocks
{
    public class MockEntraIdProvider : IEntraIdProvider
    {
        private readonly AttendanceDbContext _context;

        public MockEntraIdProvider(AttendanceDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Employee>> SyncEmployeesAsync()
        {
            return await _context.Employees.Include(e => e.Department).Include(e => e.OfficeLocation).ToListAsync();
        }

        public async Task<IReadOnlyList<Department>> SyncDepartmentsAsync()
        {
            return await _context.Departments.ToListAsync();
        }

        public Task SyncManagerHierarchyAsync()
        {
            return Task.CompletedTask;
        }
    }

    public class MockIntuneProvider : IIntuneProvider
    {
        private readonly AttendanceDbContext _context;

        public MockIntuneProvider(AttendanceDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Device>> SyncManagedDevicesAsync()
        {
            return await _context.Devices.Include(d => d.Employee).ToListAsync();
        }

        public async Task<ComplianceStatus> GetDeviceComplianceStatusAsync(string intuneDeviceId)
        {
            var device = await _context.Devices.FirstOrDefaultAsync(d => d.IntuneDeviceId == intuneDeviceId);
            return device?.ComplianceStatus ?? ComplianceStatus.Unknown;
        }
    }

    public class MockDefenderProvider : IDefenderProvider
    {
        private readonly AttendanceDbContext _context;

        public MockDefenderProvider(AttendanceDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TelemetryEvent>> FetchLatestTelemetryEventsAsync()
        {
            return await _context.TelemetryEvents.Where(e => !e.ProcessedStatus).Take(50).ToListAsync();
        }
    }

    public class ScenarioSimulator
    {
        private readonly AttendanceDbContext _context;
        private readonly IAttendanceEngine _attendanceEngine;

        public ScenarioSimulator(AttendanceDbContext context, IAttendanceEngine attendanceEngine)
        {
            _context = context;
            _attendanceEngine = attendanceEngine;
        }

        // Scenario 1: First login of day in Ramboll Office
        public async Task TriggerOfficeLoginScenarioAsync(int employeeId)
        {
            var emp = await _context.Employees.Include(e => e.OfficeLocation).FirstOrDefaultAsync(e => e.Id == employeeId);
            if (emp == null) return;

            var device = await _context.Devices.FirstOrDefaultAsync(d => d.EmployeeId == employeeId);
            var officeLoc = emp.OfficeLocation?.Name.Split(' ')[1] ?? "CHN";

            var telemetry = new TelemetryEvent
            {
                EmployeeId = emp.Id,
                DeviceId = device?.Id,
                TelemetrySource = TelemetrySource.Defender,
                EventType = EventType.Logon,
                Timestamp = DateTime.Now,
                IPAddress = "10.100.45.101",
                NetworkSSID = $"Ramboll-{officeLoc}-Corporate",
                SubnetInfo = "10.100.0.0/16"
            };

            await _context.TelemetryEvents.AddAsync(telemetry);
            await _context.SaveChangesAsync();
            await _attendanceEngine.ProcessTelemetryEventAsync(telemetry);
        }

        // Scenario 2: Multi-Device Switch (Laptop A -> Laptop B -> Laptop A)
        public async Task TriggerMultiDeviceSwitchScenarioAsync(int employeeId)
        {
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
            if (emp == null) return;

            // Ensure employee has 2 devices
            var devices = await _context.Devices.Where(d => d.EmployeeId == employeeId).ToListAsync();
            if (devices.Count < 2)
            {
                var dev2 = new Device
                {
                    IntuneDeviceId = Guid.NewGuid().ToString(),
                    DeviceName = $"RAMBOLL-LAP-B-{emp.Id}",
                    SerialNumber = $"SN-B-{Guid.NewGuid().ToString().Substring(0, 5)}",
                    EmployeeId = emp.Id,
                    OperatingSystem = "Windows 11 Enterprise",
                    ComplianceStatus = ComplianceStatus.Compliant,
                    IsManaged = true
                };
                await _context.Devices.AddAsync(dev2);
                await _context.SaveChangesAsync();
                devices.Add(dev2);
            }

            var now = DateTime.Now;

            // Event from Laptop A
            var evt1 = new TelemetryEvent
            {
                EmployeeId = emp.Id,
                DeviceId = devices[0].Id,
                TelemetrySource = TelemetrySource.Defender,
                EventType = EventType.Heartbeat,
                Timestamp = now.AddMinutes(-30),
                IPAddress = "10.100.45.101",
                NetworkSSID = "Ramboll-CHN-Corporate"
            };
            await _context.TelemetryEvents.AddAsync(evt1);
            await _context.SaveChangesAsync();
            await _attendanceEngine.ProcessTelemetryEventAsync(evt1);

            // Event from Laptop B
            var evt2 = new TelemetryEvent
            {
                EmployeeId = emp.Id,
                DeviceId = devices[1].Id,
                TelemetrySource = TelemetrySource.Defender,
                EventType = EventType.Heartbeat,
                Timestamp = now,
                IPAddress = "10.100.45.102",
                NetworkSSID = "Ramboll-CHN-Corporate"
            };
            await _context.TelemetryEvents.AddAsync(evt2);
            await _context.SaveChangesAsync();
            await _attendanceEngine.ProcessTelemetryEventAsync(evt2);
        }

        // Scenario 3: Non-compliant device telemetry attempt
        public async Task TriggerNonCompliantDeviceScenarioAsync(int employeeId)
        {
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
            if (emp == null) return;

            var unmanagedDev = new Device
            {
                IntuneDeviceId = Guid.NewGuid().ToString(),
                DeviceName = "PERSONAL-BYOD-LAPTOP",
                SerialNumber = "SN-BYOD-999",
                EmployeeId = emp.Id,
                OperatingSystem = "Windows 10 Home",
                ComplianceStatus = ComplianceStatus.NonCompliant,
                IsManaged = false
            };
            await _context.Devices.AddAsync(unmanagedDev);
            await _context.SaveChangesAsync();

            var evt = new TelemetryEvent
            {
                EmployeeId = emp.Id,
                DeviceId = unmanagedDev.Id,
                TelemetrySource = TelemetrySource.Defender,
                EventType = EventType.Logon,
                Timestamp = DateTime.Now,
                IPAddress = "10.100.45.200",
                NetworkSSID = "Ramboll-CHN-Corporate"
            };
            await _context.TelemetryEvents.AddAsync(evt);
            await _context.SaveChangesAsync();
            await _attendanceEngine.ProcessTelemetryEventAsync(evt);
        }

        // Scenario 4: Remote VPN session (First/Last seen recorded, Office hours = 0)
        public async Task TriggerRemoteVPNDayScenarioAsync(int employeeId)
        {
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
            if (emp == null) return;

            var dev = await _context.Devices.FirstOrDefaultAsync(d => d.EmployeeId == employeeId);

            var evt = new TelemetryEvent
            {
                EmployeeId = emp.Id,
                DeviceId = dev?.Id,
                TelemetrySource = TelemetrySource.EntraID,
                EventType = EventType.Logon,
                Timestamp = DateTime.Now,
                IPAddress = "192.168.1.15",
                NetworkSSID = "Home-WiFi-5G"
            };
            await _context.TelemetryEvents.AddAsync(evt);
            await _context.SaveChangesAsync();
            await _attendanceEngine.ProcessTelemetryEventAsync(evt);
        }
    }
}
