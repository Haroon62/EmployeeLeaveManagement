namespace EmployeeLeaveManagement.Application.LeaveBalances;

public sealed record LeaveBalanceDto(
    Guid Id,
    Guid EmployeeId,
    Guid LeaveTypeId,
    string LeaveTypeName,
    int Year,
    int AllocatedDays,
    int UsedDays,
    int RemainingDays);
