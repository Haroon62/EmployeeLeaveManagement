namespace EmployeeLeaveManagement.Application.LeaveTypes;

public interface ILeaveTypeService
{
    Task<IReadOnlyList<LeaveTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<LeaveTypeDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<LeaveTypeDto> CreateAsync(
        CreateLeaveTypeRequest request,
        CancellationToken cancellationToken = default);

    Task<LeaveTypeDto> UpdateAsync(
        Guid id,
        UpdateLeaveTypeRequest request,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
