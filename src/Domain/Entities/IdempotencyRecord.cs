using EmployeeLeaveManagement.Domain.Common;

namespace EmployeeLeaveManagement.Domain.Entities;

public sealed class IdempotencyRecord : BaseEntity
{
    public Guid UserId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string RequestHash { get; set; } = string.Empty;

    public Guid? LeaveRequestId { get; set; }

    public User User { get; set; } = null!;

    public LeaveRequest? LeaveRequest { get; set; }
}
