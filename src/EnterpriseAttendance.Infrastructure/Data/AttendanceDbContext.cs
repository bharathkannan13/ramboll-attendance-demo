using Microsoft.EntityFrameworkCore;
using EnterpriseAttendance.Core.Entities;

namespace EnterpriseAttendance.Infrastructure.Data
{
    public class AttendanceDbContext : DbContext
    {
        public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<OfficeLocation> OfficeLocations => Set<OfficeLocation>();
        public DbSet<OfficeNetwork> OfficeNetworks => Set<OfficeNetwork>();
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<AttendanceSession> AttendanceSessions => Set<AttendanceSession>();
        public DbSet<DailyAttendance> DailyAttendances => Set<DailyAttendance>();
        public DbSet<AttendanceSummary> AttendanceSummaries => Set<AttendanceSummary>();
        public DbSet<TelemetryEvent> TelemetryEvents => Set<TelemetryEvent>();
        public DbSet<BusinessRule> BusinessRules => Set<BusinessRule>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<ApiLog> ApiLogs => Set<ApiLog>();
        public DbSet<EmailNotificationLog> EmailNotificationLogs => Set<EmailNotificationLog>();
        public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
        public DbSet<WeeklyReportLog> WeeklyReportLogs => Set<WeeklyReportLog>();
        public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee self-referencing relationship for Org Chart hierarchy
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Manager)
                .WithMany(m => m.DirectReports)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee to Department
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee to OfficeLocation
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.OfficeLocation)
                .WithMany(o => o.Employees)
                .HasForeignKey(e => e.OfficeLocationId)
                .OnDelete(DeleteBehavior.SetNull);

            // Department Head
            modelBuilder.Entity<Department>()
                .HasOne(d => d.DepartmentHead)
                .WithMany()
                .HasForeignKey(d => d.DepartmentHeadId)
                .OnDelete(DeleteBehavior.Restrict);

            // OfficeLocation to OfficeNetwork
            modelBuilder.Entity<OfficeNetwork>()
                .HasOne(n => n.OfficeLocation)
                .WithMany(o => o.OfficeNetworks)
                .HasForeignKey(n => n.OfficeLocationId)
                .OnDelete(DeleteBehavior.Cascade);

            // AttendanceSession relations
            modelBuilder.Entity<AttendanceSession>()
                .HasOne(s => s.Employee)
                .WithMany(e => e.AttendanceSessions)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AttendanceSession>()
                .HasOne(s => s.Device)
                .WithMany(d => d.Sessions)
                .HasForeignKey(s => s.DeviceId)
                .OnDelete(DeleteBehavior.SetNull);

            // DailyAttendance relations
            modelBuilder.Entity<DailyAttendance>()
                .HasOne(d => d.Employee)
                .WithMany(e => e.DailyAttendances)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique indexes
            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.EntraObjectId)
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.Email)
                .IsUnique();

            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Code)
                .IsUnique();

            modelBuilder.Entity<Device>()
                .HasIndex(d => d.IntuneDeviceId)
                .IsUnique();
        }
    }
}
