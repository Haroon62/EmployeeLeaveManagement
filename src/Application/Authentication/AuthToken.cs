namespace EmployeeLeaveManagement.Application.Authentication;

public sealed record AuthToken(string Value, DateTime ExpiresAtUtc);
