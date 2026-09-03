using EmployeeLeaveManagement.Domain.Enums;

namespace EmployeeLeaveManagement.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }

    string JwtId { get; }

    UserRole Role { get; }

    DateTime TokenExpiresAtUtc { get; }
}
