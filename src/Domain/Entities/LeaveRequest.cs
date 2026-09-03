using EmployeeLeaveManagement.Domain.Common;
using EmployeeLeaveManagement.Domain.Enums;

namespace EmployeeLeaveManagement.Domain.Entities;

public sealed class LeaveRequest : BaseEntity
{
    public Guid EmployeeId { get; set; }

    public Guid LeaveTypeId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int WorkingDays { get; set; }

    public string Reason { get; set; } = string.Empty;

    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

    public string? RejectionReason { get; set; }

    public Guid? ReviewedByEmployeeId { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }

    public DateTime? CancelledAtUtc { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public Employee Employee { get; set; } = null!;

    public LeaveType LeaveType { get; set; } = null!;

    public Employee? ReviewedByEmployee { get; set; }

    public IdempotencyRecord? IdempotencyRecord { get; set; }
}
