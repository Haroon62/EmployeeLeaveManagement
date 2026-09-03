using EmployeeLeaveManagement.API.Services;
using EmployeeLeaveManagement.Application.LeaveRequests;
using EmployeeLeaveManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveManagement.API.Controllers;

[ApiController, Route("api/leave-requests"), Authorize]
public sealed class LeaveRequestsController(ILeaveRequestService service, ICurrentUser user) : ControllerBase
{
    [HttpGet] public Task<IReadOnlyList<LeaveRequestStatusDto>> GetAll(CancellationToken ct) => service.GetAllAsync(user.UserId, user.Role, ct);
    [HttpGet("{id:guid}")] public Task<LeaveRequestStatusDto> Get(Guid id, CancellationToken ct) => service.GetByIdAsync(id, user.UserId, user.Role, ct);
    [HttpPost] public async Task<ActionResult<LeaveRequestStatusDto>> Create(CreateLeaveRequest request, CancellationToken ct) { var result = await service.CreateAsync(user.UserId, request, Request.Headers["Idempotency-Key"].FirstOrDefault() ?? string.Empty, ct); return CreatedAtAction(nameof(Get), new { id = result.Id }, result); }
    [HttpPut("{id:guid}/cancel")] public async Task<IActionResult> Cancel(Guid id, CancellationToken ct) { await service.CancelAsync(id, user.UserId, ct); return NoContent(); }
    [HttpPut("{id:guid}/approve"), Authorize(Roles = "Manager")] public async Task<IActionResult> Approve(Guid id, CancellationToken ct) { await service.ApproveAsync(id, user.UserId, ct); return NoContent(); }
    [HttpPut("{id:guid}/reject"), Authorize(Roles = "Manager")] public async Task<IActionResult> Reject(Guid id, RejectLeaveRequest request, CancellationToken ct) { await service.RejectAsync(id, user.UserId, request.RejectionReason, ct); return NoContent(); }
}
