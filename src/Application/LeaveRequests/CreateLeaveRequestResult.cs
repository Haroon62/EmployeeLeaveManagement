namespace EmployeeLeaveManagement.Application.LeaveRequests;

public sealed record CreateLeaveRequestResult(LeaveRequestDto Request, bool WasCreated);
