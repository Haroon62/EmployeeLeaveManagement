using EmployeeLeaveManagement.Domain.Common;

namespace EmployeeLeaveManagement.Domain.Entities;

public sealed class LeaveBalance : BaseEntity
{
    public Guid EmployeeId { get; set; }

    public Guid LeaveTypeId { get; set; }

    public int Year { get; set; }

    public int AllocatedDays { get; set; }

    public int UsedDays { get; set; }

    public int RemainingDays { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public Employee Employee { get; set; } = null!;

    public LeaveType LeaveType { get; set; } = null!;
}
