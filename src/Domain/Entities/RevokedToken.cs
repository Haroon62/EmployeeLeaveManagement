using EmployeeLeaveManagement.Domain.Common;

namespace EmployeeLeaveManagement.Domain.Entities;

public sealed class RevokedToken : BaseEntity
{
    public Guid UserId { get; set; }

    public string JwtId { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public User User { get; set; } = null!;
}
