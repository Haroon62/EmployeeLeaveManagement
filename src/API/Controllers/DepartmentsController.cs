using EmployeeLeaveManagement.Application.Departments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveManagement.API.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize(Roles = "Admin,Manager")]
public sealed class DepartmentsController(IDepartmentService departmentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DepartmentDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await departmentService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DepartmentDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await departmentService.GetByIdAsync(id, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var department = await departmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DepartmentDto>> Update(
        Guid id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await departmentService.UpdateAsync(id, request, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await departmentService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
