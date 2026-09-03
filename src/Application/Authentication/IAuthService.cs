namespace EmployeeLeaveManagement.Application.Authentication;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        Guid userId,
        string jwtId,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default);
}
