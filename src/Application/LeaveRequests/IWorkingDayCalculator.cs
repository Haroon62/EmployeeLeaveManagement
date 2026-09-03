namespace EmployeeLeaveManagement.Application.LeaveRequests;

public interface IWorkingDayCalculator
{
    int Calculate(DateOnly startDate, DateOnly endDate);
}
