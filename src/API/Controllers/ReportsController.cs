using EmployeeLeaveManagement.API.Services;
using EmployeeLeaveManagement.Application.Reports;
using EmployeeLeaveManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveManagement.API.Controllers;

[ApiController, Route("api/reports"), Authorize(Roles = "Admin,Manager")]
public sealed class ReportsController(IReportService service, ICurrentUser user) : ControllerBase
{
    [HttpGet("leave-summary")] public Task<LeaveSummaryDto> Summary(int? year, CancellationToken ct) => service.GetLeaveSummaryAsync(user.UserId, user.Role, year, ct);
    [HttpGet("department-leaves")] public Task<IReadOnlyList<DepartmentLeaveDto>> Departments(int? year, CancellationToken ct) => service.GetDepartmentLeavesAsync(user.UserId, user.Role, year, ct);
    [HttpGet("monthly-leaves")] public Task<IReadOnlyList<MonthlyLeaveDto>> Monthly(int year, CancellationToken ct) => service.GetMonthlyLeavesAsync(user.UserId, user.Role, year, ct);
}
