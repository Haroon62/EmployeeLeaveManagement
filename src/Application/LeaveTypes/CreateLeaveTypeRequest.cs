using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.Application.LeaveTypes;

public sealed record CreateLeaveTypeRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }

    [Range(0, 365)]
    public int DefaultAnnualDays { get; init; }
}
