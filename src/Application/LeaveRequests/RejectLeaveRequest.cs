using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.Application.LeaveRequests;

public sealed record RejectLeaveRequest
{
    [Required, MinLength(1), MaxLength(1000)]
    public string RejectionReason { get; init; } = string.Empty;
}
