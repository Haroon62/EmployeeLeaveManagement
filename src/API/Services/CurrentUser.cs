using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EmployeeLeaveManagement.Application.Common.Exceptions;
using EmployeeLeaveManagement.Application.Common.Interfaces;
using EmployeeLeaveManagement.Domain.Enums;

namespace EmployeeLeaveManagement.API.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal Principal => httpContextAccessor.HttpContext?.User
        ?? throw new AuthenticationException("No authenticated user is available.");

    public Guid UserId => ParseGuidClaim(ClaimTypes.NameIdentifier);

    public string JwtId => Principal.FindFirstValue(JwtRegisteredClaimNames.Jti)
        ?? throw new AuthenticationException("The access token has no identifier.");

    public UserRole Role
    {
        get
        {
            var value = Principal.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(value, true, out var role)
                ? role
                : throw new AuthenticationException("The access token has an invalid role.");
        }
    }

    public DateTime TokenExpiresAtUtc
    {
        get
        {
            var value = Principal.FindFirstValue(JwtRegisteredClaimNames.Exp);
            return long.TryParse(value, out var seconds)
                ? DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime
                : throw new AuthenticationException("The access token has no valid expiry.");
        }
    }

    private Guid ParseGuidClaim(string claimType)
    {
        var value = Principal.FindFirstValue(claimType);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new AuthenticationException("The access token has an invalid user identifier.");
    }
}
