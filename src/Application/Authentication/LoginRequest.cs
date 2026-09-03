using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.Application.Authentication;

public sealed record LoginRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string Password { get; init; } = string.Empty;
}
