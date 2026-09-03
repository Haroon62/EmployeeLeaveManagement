using EmployeeLeaveManagement.Domain.Enums;

namespace EmployeeLeaveManagement.Application.Reports;

public interface IReportService
{
    Task<LeaveSummaryDto> GetLeaveSummaryAsync(
        Guid userId,
        UserRole role,
        int? year,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DepartmentLeaveDto>> GetDepartmentLeavesAsync(
        Guid userId,
        UserRole role,
        int? year,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthlyLeaveDto>> GetMonthlyLeavesAsync(
        Guid userId,
        UserRole role,
        int year,
        CancellationToken cancellationToken = default);
}
