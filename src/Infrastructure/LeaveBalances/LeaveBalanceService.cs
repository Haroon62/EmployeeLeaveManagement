using EmployeeLeaveManagement.Application.Common.Exceptions;
using EmployeeLeaveManagement.Application.LeaveBalances;
using EmployeeLeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Infrastructure.LeaveBalances;

public sealed class LeaveBalanceService(ApplicationDbContext db) : ILeaveBalanceService
{
    public async Task<IReadOnlyList<LeaveBalanceDto>> GetCurrentAsync(Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var employeeId = await db.Employees.Where(e => e.UserId == currentUserId && e.IsActive).Select(e => (Guid?)e.Id).SingleOrDefaultAsync(cancellationToken)
            ?? throw new ForbiddenException("An active employee profile is required.");
        var year = DateTime.UtcNow.Year;
        return await db.LeaveBalances.AsNoTracking().Where(b => b.EmployeeId == employeeId && b.Year == year)
            .OrderBy(b => b.LeaveType.Name).Select(b => new LeaveBalanceDto(b.Id, b.EmployeeId, b.LeaveTypeId, b.LeaveType.Name, b.Year, b.AllocatedDays, b.UsedDays, b.RemainingDays)).ToListAsync(cancellationToken);
    }
}
