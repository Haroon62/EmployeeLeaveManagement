namespace EmployeeLeaveManagement.Application.LeaveRequests;

public sealed record LeaveRequestStatusDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid LeaveTypeId,
    string LeaveTypeName,
    DateOnly StartDate,
    DateOnly EndDate,
    int WorkingDays,
    string Reason,
    string Status,
    string? RejectionReason,
    Guid? ReviewedByEmployeeId,
    DateTime? ReviewedAtUtc,
    DateTime? CancelledAtUtc,
    DateTime CreatedAtUtc);
