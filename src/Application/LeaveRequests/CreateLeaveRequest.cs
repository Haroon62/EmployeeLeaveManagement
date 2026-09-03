using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.Application.LeaveRequests;

public sealed record CreateLeaveRequest
{
    [Required]
    public Guid LeaveTypeId { get; init; }

    [Required]
    public DateOnly StartDate { get; init; }

    [Required]
    public DateOnly EndDate { get; init; }

    [Required, MaxLength(1000)]
    public string Reason { get; init; } = string.Empty;
}
