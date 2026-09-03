using EmployeeLeaveManagement.Domain.Entities;
using EmployeeLeaveManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Infrastructure.Persistence;

public sealed class DatabaseSeeder(
    ApplicationDbContext dbContext,
    IPasswordHasher<User> passwordHasher)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var department = new Department
        {
            Name = "Engineering",
            Description = "Software engineering and product delivery"
        };

        var annualLeave = new LeaveType
        {
            Name = "Annual Leave",
            Description = "Planned paid time off",
            DefaultAnnualDays = 20
        };

        var sickLeave = new LeaveType
        {
            Name = "Sick Leave",
            Description = "Leave for illness or medical care",
            DefaultAnnualDays = 10
        };

        var adminUser = CreateUser("admin@leave.local", UserRole.Admin);
        var managerUser = CreateUser("manager@leave.local", UserRole.Manager);
        var employeeUser = CreateUser("employee@leave.local", UserRole.Employee);

        HashPassword(adminUser, "Admin123!");
        HashPassword(managerUser, "Manager123!");
        HashPassword(employeeUser, "Employee123!");

        var manager = new Employee
        {
            User = managerUser,
            EmployeeNumber = "MGR-001",
            FirstName = "Maya",
            LastName = "Manager",
            Department = department
        };

        var employee = new Employee
        {
            User = employeeUser,
            EmployeeNumber = "EMP-001",
            FirstName = "Evan",
            LastName = "Employee",
            Department = department,
            Manager = manager
        };

        dbContext.Users.Add(adminUser);
        dbContext.Employees.AddRange(manager, employee);
        dbContext.LeaveTypes.AddRange(annualLeave, sickLeave);

        await dbContext.SaveChangesAsync(cancellationToken);

        department.ManagerEmployeeId = manager.Id;

        var currentYear = DateTime.UtcNow.Year;
        dbContext.LeaveBalances.AddRange(
            CreateBalance(manager, annualLeave, currentYear),
            CreateBalance(manager, sickLeave, currentYear),
            CreateBalance(employee, annualLeave, currentYear),
            CreateBalance(employee, sickLeave, currentYear));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static User CreateUser(string email, UserRole role)
    {
        return new User
        {
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            Role = role
        };
    }

    private void HashPassword(User user, string password)
    {
        user.PasswordHash = passwordHasher.HashPassword(user, password);
    }

    private static LeaveBalance CreateBalance(Employee employee, LeaveType leaveType, int year)
    {
        return new LeaveBalance
        {
            Employee = employee,
            LeaveType = leaveType,
            Year = year,
            AllocatedDays = leaveType.DefaultAnnualDays,
            RemainingDays = leaveType.DefaultAnnualDays
        };
    }
}
