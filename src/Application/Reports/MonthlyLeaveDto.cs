namespace EmployeeLeaveManagement.Application.Reports;

public sealed record MonthlyLeaveDto(
    int Year,
    int Month,
    string MonthName,
    int ApprovedRequestCount,
    int ApprovedWorkingDays);
