namespace EmployeeLeaveManagement.Application.Authentication;

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string Role,
    string AccessToken,
    DateTime ExpiresAtUtc);
