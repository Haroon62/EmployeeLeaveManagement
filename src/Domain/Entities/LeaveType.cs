using EmployeeLeaveManagement.Domain.Common;

namespace EmployeeLeaveManagement.Domain.Entities;

public sealed class LeaveType : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DefaultAnnualDays { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<LeaveBalance> LeaveBalances { get; set; } = [];

    public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];
}
