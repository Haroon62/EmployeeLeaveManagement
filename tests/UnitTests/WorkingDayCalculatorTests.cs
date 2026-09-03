using EmployeeLeaveManagement.Application.LeaveRequests;
#pragma warning disable CA1707

namespace EmployeeLeaveManagement.UnitTests;

public sealed class WorkingDayCalculatorTests
{
    [Fact] public void Monday_is_one_day() => Assert.Equal(1, EmployeeLeaveManagement.Application.LeaveRequests.WorkingDayCalculator.Count(new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 7)));
    [Fact] public void Weekends_are_excluded() => Assert.Equal(5, WorkingDayCalculator.Count(new(2026, 9, 7), new(2026, 9, 13)));
    [Fact] public void Saturday_and_sunday_are_zero() => Assert.Equal(0, WorkingDayCalculator.Count(new(2026, 9, 12), new(2026, 9, 13)));
    [Fact] public void Friday_to_monday_is_two_days() => Assert.Equal(2, WorkingDayCalculator.Count(new(2026, 9, 11), new(2026, 9, 14)));
    [Fact] public void Month_boundary_counts_correctly() => Assert.Equal(2, WorkingDayCalculator.Count(new(2026, 9, 30), new(2026, 10, 1)));
    [Fact] public void Leap_day_is_counted() => Assert.Equal(1, WorkingDayCalculator.Count(new(2028, 2, 29), new(2028, 2, 29)));
    [Fact] public void Full_week_is_five() => Assert.Equal(5, WorkingDayCalculator.Count(new(2026, 1, 5), new(2026, 1, 11)));
    [Fact] public void Same_weekend_day_is_zero() => Assert.Equal(0, WorkingDayCalculator.Count(new(2026, 1, 3), new(2026, 1, 3)));
    [Fact] public void Reverse_dates_throw() => Assert.Throws<ArgumentException>(() => WorkingDayCalculator.Count(new(2026, 9, 8), new(2026, 9, 7)));
    [Fact] public void Two_weeks_are_ten_days() => Assert.Equal(10, WorkingDayCalculator.Count(new(2026, 3, 2), new(2026, 3, 13)));
}
