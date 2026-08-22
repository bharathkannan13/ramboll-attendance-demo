using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using EnterpriseAttendance.Core.Entities;
using EnterpriseAttendance.Core.Enums;
using EnterpriseAttendance.Core.Interfaces;
using EnterpriseAttendance.Infrastructure.Data;

namespace EnterpriseAttendance.Services.Notifications
{
    public class ExcelReportGenerator : IReportGenerator
    {
        private readonly AttendanceDbContext _context;
        private readonly IOrgHierarchyService _orgHierarchyService;

        public ExcelReportGenerator(AttendanceDbContext context, IOrgHierarchyService orgHierarchyService)
        {
            _context = context;
            _orgHierarchyService = orgHierarchyService;
        }

        public async Task<byte[]> GenerateWeeklyManagerExcelReportAsync(int managerId, DateTime weekStartDate)
        {
            var weekEnd = weekStartDate.AddDays(6);
            var teamMembers = await _orgHierarchyService.GetReportingSubtreeAsync(managerId);
            var teamIds = teamMembers.Select(t => t.Id).ToList();

            var dailyRecords = await _context.DailyAttendances
                .Include(d => d.Employee)
                .Where(d => teamIds.Contains(d.EmployeeId) && d.AttendanceDate >= weekStartDate && d.AttendanceDate <= weekEnd)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Weekly Team Attendance");

            // Header
            worksheet.Cell(1, 1).Value = "Employee Code";
            worksheet.Cell(1, 2).Value = "Employee Name";
            worksheet.Cell(1, 3).Value = "Department";
            worksheet.Cell(1, 4).Value = "Office Location";
            worksheet.Cell(1, 5).Value = "Date";
            worksheet.Cell(1, 6).Value = "Attendance Type";
            worksheet.Cell(1, 7).Value = "First Seen (Corporate Network)";
            worksheet.Cell(1, 8).Value = "Last Seen (Corporate Network)";
            worksheet.Cell(1, 9).Value = "Office Working Hours";
            worksheet.Cell(1, 10).Value = "Compliance Status";

            var headerRange = worksheet.Range(1, 1, 1, 10);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
            headerRange.Style.Font.FontColor = XLColor.White;

            int row = 2;
            foreach (var record in dailyRecords)
            {
                worksheet.Cell(row, 1).Value = record.Employee?.EmployeeCode ?? "";
                worksheet.Cell(row, 2).Value = record.Employee?.FullName ?? "";
                worksheet.Cell(row, 3).Value = record.Employee?.Department?.Name ?? "";
                worksheet.Cell(row, 4).Value = record.OfficeLocation?.Name ?? "Remote";
                worksheet.Cell(row, 5).Value = record.AttendanceDate.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 6).Value = record.AttendanceType.ToString();
                worksheet.Cell(row, 7).Value = record.FirstSeenTime?.ToString("HH:mm:ss") ?? "N/A";
                worksheet.Cell(row, 8).Value = record.LastSeenTime?.ToString("HH:mm:ss") ?? "N/A";
                worksheet.Cell(row, 9).Value = record.TotalOfficeHours;
                worksheet.Cell(row, 10).Value = record.IsHybridCompliant ? "COMPLIANT" : "NON-COMPLIANT";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> GenerateMonthlyManagerExcelReportAsync(int managerId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var teamMembers = await _orgHierarchyService.GetReportingSubtreeAsync(managerId);
            var teamIds = teamMembers.Select(t => t.Id).ToList();

            var summaries = await _context.AttendanceSummaries
                .Include(s => s.Employee)
                .Include(s => s.Department)
                .Where(s => teamIds.Contains(s.EmployeeId) && s.PeriodType == PeriodType.Monthly && s.PeriodStartDate == startDate)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Monthly Team Summary");

            worksheet.Cell(1, 1).Value = "Employee Name";
            worksheet.Cell(1, 2).Value = "Department";
            worksheet.Cell(1, 3).Value = "Office Days Count";
            worksheet.Cell(1, 4).Value = "WFH Days Count";
            worksheet.Cell(1, 5).Value = "Total Office Hours";
            worksheet.Cell(1, 6).Value = "Avg Office Hours / Day";
            worksheet.Cell(1, 7).Value = "Monthly Compliance %";
            worksheet.Cell(1, 8).Value = "Policy Status";

            var headerRange = worksheet.Range(1, 1, 1, 8);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0F172A");
            headerRange.Style.Font.FontColor = XLColor.White;

            int row = 2;
            foreach (var sum in summaries)
            {
                worksheet.Cell(row, 1).Value = sum.Employee?.FullName ?? "";
                worksheet.Cell(row, 2).Value = sum.Department?.Name ?? "";
                worksheet.Cell(row, 3).Value = sum.OfficeDaysCount;
                worksheet.Cell(row, 4).Value = sum.WFHDaysCount;
                worksheet.Cell(row, 5).Value = sum.TotalOfficeHours;
                worksheet.Cell(row, 6).Value = sum.AverageOfficeHoursPerDay;
                worksheet.Cell(row, 7).Value = $"{sum.HybridCompliancePercentage}%";
                worksheet.Cell(row, 8).Value = sum.PolicyComplianceStatus.ToString();
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly AttendanceDbContext _context;
        private readonly IOrgHierarchyService _orgHierarchyService;

        public EmailNotificationService(AttendanceDbContext context, IOrgHierarchyService orgHierarchyService)
        {
            _context = context;
            _orgHierarchyService = orgHierarchyService;
        }

        public async Task SendWeeklyManagerReportAsync(int managerId, DateTime weekStartDate)
        {
            var manager = await _context.Employees.FindAsync(managerId);
            if (manager == null) return;

            var weekEnd = weekStartDate.AddDays(6);
            var teamMembers = await _orgHierarchyService.GetReportingSubtreeAsync(managerId);
            var teamIds = teamMembers.Select(t => t.Id).ToList();

            var dailyRecords = await _context.DailyAttendances
                .Include(d => d.Employee)
                .Where(d => teamIds.Contains(d.EmployeeId) && d.AttendanceDate >= weekStartDate && d.AttendanceDate <= weekEnd)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"<h2>Bkran Group Connect — Attendance Report ({manager.FullName}'s Team)</h2>");
            sb.AppendLine($"<p>Period: <strong>{weekStartDate:MMM dd, yyyy} to {weekEnd:MMM dd, yyyy}</strong></p>");
            sb.AppendLine("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse:collapse; font-family:sans-serif;'>");
            sb.AppendLine("<tr style='background-color:#1E293B; color:#ffffff;'><th>Employee Name</th><th>Office Days</th><th>WFH Days</th><th>Total Office Hours</th><th>Status</th></tr>");

            foreach (var emp in teamMembers)
            {
                var empRecords = dailyRecords.Where(r => r.EmployeeId == emp.Id).ToList();
                int officeDays = empRecords.Count(r => r.AttendanceType == AttendanceType.Office);
                int wfhDays = empRecords.Count(r => r.AttendanceType == AttendanceType.WFH);
                double hours = empRecords.Sum(r => r.TotalOfficeHours);
                string statusColor = officeDays >= 3 ? "#10B981" : (officeDays == 2 ? "#F59E0B" : "#EF4444");
                string statusText = officeDays >= 3 ? "MET (3/3)" : (officeDays == 2 ? "PARTIAL (2/3)" : "NON-COMPLIANT");

                sb.AppendLine($"<tr><td>{emp.FullName}</td><td>{officeDays}</td><td>{wfhDays}</td><td>{hours:F1} hrs</td><td style='color:{statusColor}; font-weight:bold;'>{statusText}</td></tr>");
            }
            sb.AppendLine("</table>");
            sb.AppendLine("<br/><p>Log in to your <strong>Bkran Group Connect Manager Dashboard</strong> to view individual employee timelines and detailed Network First/Last seen records.</p>");

            var log = new EmailNotificationLog
            {
                RecipientEmployeeId = managerId,
                RecipientEmail = manager.Email,
                NotificationType = "WeeklyManagerReport",
                Subject = $"Bkran Group Connect — Team Attendance Report ({weekStartDate:MMM dd})",
                BodyHtml = sb.ToString(),
                DeliveryStatus = "PreviewInbox"
            };

            await _context.EmailNotificationLogs.AddAsync(log);

            var reportLog = new WeeklyReportLog
            {
                ManagerId = managerId,
                DirectReportCount = teamMembers.Count,
                AverageAttendancePct = teamMembers.Count > 0 ? (double)teamMembers.Count(t => dailyRecords.Count(d => d.EmployeeId == t.Id && d.AttendanceType == AttendanceType.Office) >= 3) / teamMembers.Count * 100 : 0,
                ReportHtmlContent = sb.ToString(),
                DeliveryStatus = "PreviewInbox"
            };
            await _context.WeeklyReportLogs.AddAsync(reportLog);
            await _context.SaveChangesAsync();
        }

        public async Task SendMonthlyManagerSummaryAsync(int managerId, int year, int month)
        {
            var manager = await _context.Employees.FindAsync(managerId);
            if (manager == null) return;

            var startDate = new DateTime(year, month, 1);
            var teamMembers = await _orgHierarchyService.GetReportingSubtreeAsync(managerId);

            var sb = new StringBuilder();
            sb.AppendLine($"<h2>Bkran Group Connect — Monthly Attendance Summary ({startDate:MMMM yyyy})</h2>");
            sb.AppendLine($"<p>Manager: <strong>{manager.FullName}</strong> | Total Team Size: <strong>{teamMembers.Count}</strong></p>");

            var log = new EmailNotificationLog
            {
                RecipientEmployeeId = managerId,
                RecipientEmail = manager.Email,
                NotificationType = "MonthlyManagerSummary",
                Subject = $"Bkran Group Connect — Monthly Attendance Summary ({startDate:MMMM yyyy})",
                BodyHtml = sb.ToString(),
                DeliveryStatus = "PreviewInbox"
            };

            await _context.EmailNotificationLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<EmailNotificationLog>> GetEmailLogsAsync()
        {
            return await _context.EmailNotificationLogs.OrderByDescending(l => l.SentAt).ToListAsync();
        }
    }
}
