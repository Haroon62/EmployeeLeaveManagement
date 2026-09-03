using EmployeeLeaveManagement.Domain.Enums;

namespace EmployeeLeaveManagement.Application.LeaveRequests;

public interface ILeaveRequestService
{
    Task<IReadOnlyList<LeaveRequestStatusDto>> GetAllAsync(
        Guid currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<LeaveRequestStatusDto> GetByIdAsync(
        Guid id,
        Guid currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default);

    Task<LeaveRequestStatusDto> CreateAsync(
        Guid currentUserId,
        CreateLeaveRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task CancelAsync(
        Guid id,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    Task ApproveAsync(
        Guid id,
        Guid managerUserId,
        CancellationToken cancellationToken = default);

    Task RejectAsync(
        Guid id,
        Guid managerUserId,
        string rejectionReason,
        CancellationToken cancellationToken = default);
}
