using System.Globalization;
#pragma warning disable CA1725
using EmployeeLeaveManagement.Application.Reports;
using EmployeeLeaveManagement.Domain.Enums;
using EmployeeLeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Infrastructure.Reports;

public sealed class ReportService(ApplicationDbContext db) : IReportService
{
    public async Task<LeaveSummaryDto> GetLeaveSummaryAsync(Guid userId, UserRole role, int? year, CancellationToken ct = default)
    {
        var q = db.LeaveRequests.AsNoTracking();
        if (role == UserRole.Manager) q = q.Where(r => r.Employee.Department.ManagerEmployee != null && r.Employee.Department.ManagerEmployee.UserId == userId);
        var items = await q.ToListAsync(ct);
        return new(items.Count(x => x.Status == LeaveRequestStatus.Pending), items.Count(x => x.Status == LeaveRequestStatus.Approved), items.Count(x => x.Status == LeaveRequestStatus.Rejected), items.Count(x => x.Status == LeaveRequestStatus.Cancelled), items.Count);
    }

    public async Task<IReadOnlyList<DepartmentLeaveDto>> GetDepartmentLeavesAsync(Guid userId, UserRole role, int? year, CancellationToken ct = default)
    {
        var y = year ?? DateTime.UtcNow.Year; var q = db.LeaveRequests.AsNoTracking().Where(r => r.Status == LeaveRequestStatus.Approved && r.StartDate.Year == y);
        if (role == UserRole.Manager) q = q.Where(r => r.Employee.Department.ManagerEmployee != null && r.Employee.Department.ManagerEmployee.UserId == userId);
        return await q.GroupBy(r => new { r.Employee.DepartmentId, r.Employee.Department.Name }).Select(g => new DepartmentLeaveDto(g.Key.DepartmentId, g.Key.Name, g.Count(), g.Sum(r => r.WorkingDays))).OrderBy(x => x.DepartmentName).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MonthlyLeaveDto>> GetMonthlyLeavesAsync(Guid userId, UserRole role, int year, CancellationToken ct = default)
    {
        var q = db.LeaveRequests.AsNoTracking().Where(r => r.Status == LeaveRequestStatus.Approved && r.StartDate.Year <= year && r.EndDate.Year >= year);
        if (role == UserRole.Manager) q = q.Where(r => r.Employee.Department.ManagerEmployee != null && r.Employee.Department.ManagerEmployee.UserId == userId);
        var rows = await q.Select(r => new { r.StartDate, r.EndDate }).ToListAsync(ct); var result = new List<MonthlyLeaveDto>();
        for (var m=1;m<=12;m++) { var days=0; foreach(var r in rows) { var s = r.StartDate > new DateOnly(year,m,1) ? r.StartDate : new DateOnly(year,m,1); var e = r.EndDate < new DateOnly(year,m,DateTime.DaysInMonth(year,m)) ? r.EndDate : new DateOnly(year,m,DateTime.DaysInMonth(year,m)); if(e>=s) for(var d=s;d<=e;d=d.AddDays(1)) if(d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday) days++; } if(days>0) result.Add(new(year,m,CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(m),0,days)); }
        return result;
    }
}
