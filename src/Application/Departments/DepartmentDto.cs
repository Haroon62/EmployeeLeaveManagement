namespace EmployeeLeaveManagement.Application.Departments;

public sealed record DepartmentDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ManagerEmployeeId,
    string? ManagerName,
    bool IsActive,
    int EmployeeCount);
