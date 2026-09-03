namespace EmployeeLeaveManagement.Application.LeaveRequests;

public sealed record LeaveRequestDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeNumber,
    string EmployeeName,
    Guid DepartmentId,
    string DepartmentName,
    Guid LeaveTypeId,
    string LeaveTypeName,
    DateOnly StartDate,
    DateOnly EndDate,
    int WorkingDays,
    string Reason,
    string Status,
    string? RejectionReason,
    Guid? ReviewedByEmployeeId,
    string? ReviewedByName,
    DateTime? ReviewedAtUtc,
    DateTime? CancelledAtUtc,
    DateTime CreatedAtUtc);
