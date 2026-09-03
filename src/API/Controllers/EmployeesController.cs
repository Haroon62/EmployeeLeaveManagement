using EmployeeLeaveManagement.Application.Common.Interfaces;
using EmployeeLeaveManagement.Application.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveManagement.API.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize(Roles = "Admin,Manager")]
public sealed class EmployeesController(
    IEmployeeService employeeService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await employeeService.GetAllAsync(
            currentUser.UserId,
            currentUser.Role,
            cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await employeeService.GetByIdAsync(
            id,
            currentUser.UserId,
            currentUser.Role,
            cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmployeeDto>> Update(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await employeeService.UpdateAsync(id, request, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await employeeService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
