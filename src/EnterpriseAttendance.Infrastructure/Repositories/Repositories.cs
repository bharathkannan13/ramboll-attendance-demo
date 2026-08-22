using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EnterpriseAttendance.Core.Entities;
using EnterpriseAttendance.Core.Interfaces;
using EnterpriseAttendance.Infrastructure.Data;

namespace EnterpriseAttendance.Infrastructure.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly AttendanceDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AttendanceDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly AttendanceDbContext _context;

        public UnitOfWork(AttendanceDbContext context)
        {
            _context = context;
            Employees = new GenericRepository<Employee>(_context);
            Departments = new GenericRepository<Department>(_context);
            OfficeLocations = new GenericRepository<OfficeLocation>(_context);
            OfficeNetworks = new GenericRepository<OfficeNetwork>(_context);
            Devices = new GenericRepository<Device>(_context);
            AttendanceSessions = new GenericRepository<AttendanceSession>(_context);
            DailyAttendances = new GenericRepository<DailyAttendance>(_context);
            AttendanceSummaries = new GenericRepository<AttendanceSummary>(_context);
            TelemetryEvents = new GenericRepository<TelemetryEvent>(_context);
            BusinessRules = new GenericRepository<BusinessRule>(_context);
            AuditLogs = new GenericRepository<AuditLog>(_context);
            ApiLogs = new GenericRepository<ApiLog>(_context);
            EmailNotificationLogs = new GenericRepository<EmailNotificationLog>(_context);
            EmailTemplates = new GenericRepository<EmailTemplate>(_context);
            WeeklyReportLogs = new GenericRepository<WeeklyReportLog>(_context);
            SystemConfigurations = new GenericRepository<SystemConfiguration>(_context);
        }

        public IRepository<Employee> Employees { get; }
        public IRepository<Department> Departments { get; }
        public IRepository<OfficeLocation> OfficeLocations { get; }
        public IRepository<OfficeNetwork> OfficeNetworks { get; }
        public IRepository<Device> Devices { get; }
        public IRepository<AttendanceSession> AttendanceSessions { get; }
        public IRepository<DailyAttendance> DailyAttendances { get; }
        public IRepository<AttendanceSummary> AttendanceSummaries { get; }
        public IRepository<TelemetryEvent> TelemetryEvents { get; }
        public IRepository<BusinessRule> BusinessRules { get; }
        public IRepository<AuditLog> AuditLogs { get; }
        public IRepository<ApiLog> ApiLogs { get; }
        public IRepository<EmailNotificationLog> EmailNotificationLogs { get; }
        public IRepository<EmailTemplate> EmailTemplates { get; }
        public IRepository<WeeklyReportLog> WeeklyReportLogs { get; }
        public IRepository<SystemConfiguration> SystemConfigurations { get; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
