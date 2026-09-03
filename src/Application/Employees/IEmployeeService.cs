using EmployeeLeaveManagement.Domain.Enums;

namespace EmployeeLeaveManagement.Application.Employees;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeDto>> GetAllAsync(
        Guid currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<EmployeeDto> GetByIdAsync(
        Guid id,
        Guid currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<EmployeeDto> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default);

    Task<EmployeeDto> UpdateAsync(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
