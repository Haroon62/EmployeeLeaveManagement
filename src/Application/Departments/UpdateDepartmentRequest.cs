using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.Application.Departments;

public sealed record UpdateDepartmentRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }

    public Guid? ManagerEmployeeId { get; init; }

    public bool IsActive { get; init; }
}
