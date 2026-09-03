using EmployeeLeaveManagement.Application.Common.Exceptions;
using EmployeeLeaveManagement.Application.Employees;
using EmployeeLeaveManagement.Domain.Entities;
using EmployeeLeaveManagement.Domain.Enums;
using EmployeeLeaveManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Infrastructure.Employees;

public sealed class EmployeeService(
    ApplicationDbContext dbContext,
    IPasswordHasher<User> passwordHasher) : IEmployeeService
{
    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(
        Guid currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Employees.AsNoTracking();

        if (currentUserRole == UserRole.Manager)
        {
            var departmentId = await GetDepartmentIdAsync(currentUserId, cancellationToken);
            query = query.Where(employee => employee.DepartmentId == departmentId);
        }

        return await Project(query.OrderBy(employee => employee.EmployeeNumber))
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeDto> GetByIdAsync(
        Guid id,
        Guid currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Employees.AsNoTracking().Where(employee => employee.Id == id);

        if (currentUserRole == UserRole.Manager)
        {
            var departmentId = await GetDepartmentIdAsync(currentUserId, cancellationToken);
            query = query.Where(employee => employee.DepartmentId == departmentId);
        }

        return await Project(query).SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Employee not found.");
    }

    public async Task<EmployeeDto> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureEmployeeRole(request.Role);
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var employeeNumber = request.EmployeeNumber.Trim().ToUpperInvariant();

        if (await dbContext.Users.AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        if (await dbContext.Employees.AnyAsync(
                employee => employee.EmployeeNumber == employeeNumber,
                cancellationToken))
        {
            throw new ConflictException("An employee with this employee number already exists.");
        }

        var department = await GetActiveDepartmentAsync(request.DepartmentId, cancellationToken);
        var manager = await GetValidManagerAsync(request.ManagerId, department.Id, cancellationToken);

        var user = new User
        {
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            Role = request.Role
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        var employee = new Employee
        {
            User = user,
            EmployeeNumber = employeeNumber,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Department = department,
            Manager = manager
        };

        var leaveTypes = await dbContext.LeaveTypes
            .Where(leaveType => leaveType.IsActive)
            .ToListAsync(cancellationToken);
        foreach (var leaveType in leaveTypes)
        {
            employee.LeaveBalances.Add(new LeaveBalance
            {
                LeaveType = leaveType,
                Year = DateTime.UtcNow.Year,
                AllocatedDays = leaveType.DefaultAnnualDays,
                RemainingDays = leaveType.DefaultAnnualDays
            });
        }

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdForAdminAsync(employee.Id, cancellationToken);
    }

    public async Task<EmployeeDto> UpdateAsync(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureEmployeeRole(request.Role);
        var employee = await dbContext.Employees
            .Include(item => item.User)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Employee not found.");

        if (request.ManagerId == id)
        {
            throw new ConflictException("An employee cannot be their own manager.");
        }

        var department = await GetActiveDepartmentAsync(request.DepartmentId, cancellationToken);
        var manager = await GetValidManagerAsync(request.ManagerId, department.Id, cancellationToken);

        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.Department = department;
        employee.Manager = manager;
        employee.IsActive = request.IsActive;
        employee.User.IsActive = request.IsActive;
        employee.User.Role = request.Role;

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdForAdminAsync(employee.Id, cancellationToken);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees
            .Include(item => item.User)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Employee not found.");

        if (await dbContext.Departments.AnyAsync(
                department => department.ManagerEmployeeId == id && department.IsActive,
                cancellationToken))
        {
            throw new ConflictException("Reassign the managed department before deactivating this employee.");
        }

        employee.IsActive = false;
        employee.User.IsActive = false;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid> GetDepartmentIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Employees
            .Where(employee => employee.UserId == userId && employee.IsActive)
            .Select(employee => employee.DepartmentId)
            .SingleOrDefaultAsync(cancellationToken) is var departmentId && departmentId != Guid.Empty
                ? departmentId
                : throw new ForbiddenException("The manager does not have an active employee profile.");
    }

    private async Task<Department> GetActiveDepartmentAsync(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Departments.SingleOrDefaultAsync(
            department => department.Id == departmentId && department.IsActive,
            cancellationToken) ?? throw new NotFoundException("Department not found.");
    }

    private async Task<Employee?> GetValidManagerAsync(
        Guid? managerId,
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        if (managerId is null)
        {
            return null;
        }

        var manager = await dbContext.Employees
            .Include(employee => employee.User)
            .SingleOrDefaultAsync(employee => employee.Id == managerId, cancellationToken)
            ?? throw new NotFoundException("Manager not found.");

        if (!manager.IsActive || manager.DepartmentId != departmentId || manager.User.Role != UserRole.Manager)
        {
            throw new ConflictException("The selected manager must be an active manager in the same department.");
        }

        return manager;
    }

    private Task<EmployeeDto> GetByIdForAdminAsync(Guid id, CancellationToken cancellationToken)
    {
        return Project(dbContext.Employees.AsNoTracking().Where(employee => employee.Id == id))
            .SingleAsync(cancellationToken);
    }

    private static IQueryable<EmployeeDto> Project(IQueryable<Employee> query)
    {
        return query.Select(employee => new EmployeeDto(
            employee.Id,
            employee.UserId,
            employee.EmployeeNumber,
            employee.FirstName,
            employee.LastName,
            employee.User.Email,
            employee.User.Role.ToString(),
            employee.DepartmentId,
            employee.Department.Name,
            employee.ManagerId,
            employee.Manager == null
                ? null
                : employee.Manager.FirstName + " " + employee.Manager.LastName,
            employee.IsActive));
    }

    private static void EnsureEmployeeRole(UserRole role)
    {
        if (role is not (UserRole.Employee or UserRole.Manager))
        {
            throw new ConflictException("Employee accounts can only use the Employee or Manager role.");
        }
    }
}
