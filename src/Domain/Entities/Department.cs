using EmployeeLeaveManagement.Domain.Common;

namespace EmployeeLeaveManagement.Domain.Entities;

public sealed class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid? ManagerEmployeeId { get; set; }

    public bool IsActive { get; set; } = true;

    public Employee? ManagerEmployee { get; set; }

    public ICollection<Employee> Employees { get; set; } = [];
}
