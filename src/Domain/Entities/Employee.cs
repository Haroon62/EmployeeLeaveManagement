using EmployeeLeaveManagement.Domain.Common;

namespace EmployeeLeaveManagement.Domain.Entities;

public sealed class Employee : BaseEntity
{
    public Guid UserId { get; set; }

    public string EmployeeNumber { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }

    public Guid? ManagerId { get; set; }

    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;

    public Department Department { get; set; } = null!;

    public Employee? Manager { get; set; }

    public ICollection<Employee> DirectReports { get; set; } = [];

    public ICollection<Department> ManagedDepartments { get; set; } = [];

    public ICollection<LeaveBalance> LeaveBalances { get; set; } = [];

    public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];

    public ICollection<LeaveRequest> ReviewedLeaveRequests { get; set; } = [];
}
