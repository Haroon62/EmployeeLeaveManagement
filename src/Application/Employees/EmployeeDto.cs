namespace EmployeeLeaveManagement.Application.Employees;

public sealed record EmployeeDto(
    Guid Id,
    Guid UserId,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    Guid DepartmentId,
    string DepartmentName,
    Guid? ManagerId,
    string? ManagerName,
    bool IsActive);
