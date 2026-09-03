using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.Application.Authentication;

public sealed record RegisterRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; init; } = string.Empty;

    [Required, MaxLength(30)]
    public string EmployeeNumber { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required]
    public Guid DepartmentId { get; init; }
}
