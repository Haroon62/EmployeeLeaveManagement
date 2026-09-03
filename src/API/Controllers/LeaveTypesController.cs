using EmployeeLeaveManagement.Application.LeaveTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveManagement.API.Controllers;

[ApiController]
[Route("api/leave-types")]
[Authorize]
public sealed class LeaveTypesController(ILeaveTypeService leaveTypeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LeaveTypeDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await leaveTypeService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LeaveTypeDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await leaveTypeService.GetByIdAsync(id, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<LeaveTypeDto>> Create(
        CreateLeaveTypeRequest request,
        CancellationToken cancellationToken)
    {
        var leaveType = await leaveTypeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = leaveType.Id }, leaveType);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LeaveTypeDto>> Update(
        Guid id,
        UpdateLeaveTypeRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await leaveTypeService.UpdateAsync(id, request, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await leaveTypeService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
