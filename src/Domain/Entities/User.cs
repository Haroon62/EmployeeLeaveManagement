using EmployeeLeaveManagement.Domain.Common;
using EmployeeLeaveManagement.Domain.Enums;

namespace EmployeeLeaveManagement.Domain.Entities;

public sealed class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public string NormalizedEmail { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public Employee? Employee { get; set; }

    public ICollection<RevokedToken> RevokedTokens { get; set; } = [];

    public ICollection<IdempotencyRecord> IdempotencyRecords { get; set; } = [];
}
