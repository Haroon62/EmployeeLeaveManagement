namespace EmployeeLeaveManagement.Application.LeaveTypes;

public sealed record LeaveTypeDto(
    Guid Id,
    string Name,
    string? Description,
    int DefaultAnnualDays,
    bool IsActive);
