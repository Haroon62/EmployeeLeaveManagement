namespace EmployeeLeaveManagement.Application.Reports;

public sealed record DepartmentLeaveDto(
    Guid DepartmentId,
    string DepartmentName,
    int ApprovedRequestCount,
    int ApprovedWorkingDays);
