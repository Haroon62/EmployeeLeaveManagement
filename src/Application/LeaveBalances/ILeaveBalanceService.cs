namespace EmployeeLeaveManagement.Application.LeaveBalances;

public interface ILeaveBalanceService
{
    Task<IReadOnlyList<LeaveBalanceDto>> GetCurrentAsync(
        Guid currentUserId,
        CancellationToken cancellationToken = default);
}
