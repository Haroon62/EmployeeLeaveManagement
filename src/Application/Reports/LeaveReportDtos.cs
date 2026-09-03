namespace EmployeeLeaveManagement.Application.Reports;

public sealed record LeaveSummaryDto(
    int Pending,
    int Approved,
    int Rejected,
    int Cancelled,
    int Total);

public sealed record DepartmentLeaveSummaryDto(
    Guid DepartmentId,
    string DepartmentName,
    int ApprovedWorkingDays,
    int ApprovedRequestCount);

public sealed record MonthlyLeaveSummaryDto(
    int Year,
    int Month,
    string MonthName,
    int ApprovedWorkingDays,
    int ApprovedRequestCount);
