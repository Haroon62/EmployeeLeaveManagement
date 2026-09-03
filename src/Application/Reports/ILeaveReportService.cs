namespace EmployeeLeaveManagement.Application.Reports;

public interface ILeaveReportService
{
    Task<LeaveSummaryDto> GetSummaryAsync(
        Guid? employeeId,
        Guid? departmentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DepartmentLeaveSummaryDto>> GetDepartmentLeavesAsync(
        int year,
        Guid? departmentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthlyLeaveSummaryDto>> GetMonthlyLeavesAsync(
        int year,
        Guid? departmentId,
        CancellationToken cancellationToken = default);
}
