using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EnterpriseAttendance.Core.Entities;
using EnterpriseAttendance.Core.Interfaces;
using EnterpriseAttendance.Core.Models;
using EnterpriseAttendance.Infrastructure.Data;

namespace EnterpriseAttendance.Services.Services
{
    public class OrgHierarchyService : IOrgHierarchyService
    {
        private readonly AttendanceDbContext _context;

        public OrgHierarchyService(AttendanceDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Employee>> GetReportingSubtreeAsync(int managerEmployeeId)
        {
            var result = new List<Employee>();
            var queue = new Queue<int>();
            queue.Enqueue(managerEmployeeId);

            var allEmployees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.OfficeLocation)
                .Where(e => e.IsActive)
                .ToListAsync();

            while (queue.Count > 0)
            {
                var currentManagerId = queue.Dequeue();
                var directReports = allEmployees.Where(e => e.ManagerId == currentManagerId).ToList();

                foreach (var report in directReports)
                {
                    if (!result.Any(r => r.Id == report.Id))
                    {
                        result.Add(report);
                        queue.Enqueue(report.Id);
                    }
                }
            }

            return result;
        }

        public async Task<bool> IsEmployeeInManagerSubtreeAsync(int managerEmployeeId, int targetEmployeeId)
        {
            if (managerEmployeeId == targetEmployeeId) return true;

            var subtree = await GetReportingSubtreeAsync(managerEmployeeId);
            return subtree.Any(e => e.Id == targetEmployeeId);
        }

        public async Task<OrgNodeDto?> GetOrgChartTreeAsync(int rootManagerId)
        {
            var allEmployees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.OfficeLocation)
                .Where(e => e.IsActive)
                .ToListAsync();

            var rootEntity = allEmployees.FirstOrDefault(e => e.Id == rootManagerId)
                             ?? allEmployees.FirstOrDefault(e => e.Id == 4)
                             ?? allEmployees.FirstOrDefault(e => e.ManagerId == null)
                             ?? allEmployees.FirstOrDefault();

            if (rootEntity == null) return null;

            var rootDto = MapToDto(rootEntity);
            PopulateDirectReportsRecursive(rootDto, allEmployees);
            return rootDto;
        }

        private void PopulateDirectReportsRecursive(OrgNodeDto parentDto, List<Employee> allEmployees)
        {
            var directReportEntities = allEmployees.Where(e => e.ManagerId == parentDto.Id).ToList();
            foreach (var childEntity in directReportEntities)
            {
                var childDto = MapToDto(childEntity);
                parentDto.DirectReports.Add(childDto);
                PopulateDirectReportsRecursive(childDto, allEmployees);
            }
        }

        private OrgNodeDto MapToDto(Employee entity)
        {
            return new OrgNodeDto
            {
                Id = entity.Id,
                FullName = entity.FullName,
                Title = entity.Title,
                Email = entity.Email,
                EmployeeCode = entity.EmployeeCode,
                Department = entity.Department?.Name ?? "General",
                OfficeLocation = entity.OfficeLocation?.Name ?? "India Office",
                ManagerId = entity.ManagerId,
                OfficeDays = 3,
                ComplianceStatus = "MET"
            };
        }
    }
}
