using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.Application.Departments;

public sealed record CreateDepartmentRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }
}
