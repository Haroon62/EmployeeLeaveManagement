using EmployeeLeaveManagement.API.Services;
using EmployeeLeaveManagement.Application.LeaveBalances;
using EmployeeLeaveManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveManagement.API.Controllers;

[ApiController, Route("api/leave-balances"), Authorize]
public sealed class LeaveBalancesController(ILeaveBalanceService service, ICurrentUser user) : ControllerBase
{
    [HttpGet] public Task<IReadOnlyList<LeaveBalanceDto>> Get(CancellationToken ct) => service.GetCurrentAsync(user.UserId, ct);
}
