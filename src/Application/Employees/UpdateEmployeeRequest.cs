using System.ComponentModel.DataAnnotations;
using EmployeeLeaveManagement.Domain.Enums;

namespace EmployeeLeaveManagement.Application.Employees;

public sealed record UpdateEmployeeRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required]
    public Guid DepartmentId { get; init; }

    public Guid? ManagerId { get; init; }

    public UserRole Role { get; init; }

    public bool IsActive { get; init; }
}
