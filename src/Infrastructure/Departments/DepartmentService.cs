using EmployeeLeaveManagement.Application.Common.Exceptions;
using EmployeeLeaveManagement.Application.Departments;
using EmployeeLeaveManagement.Domain.Entities;
using EmployeeLeaveManagement.Domain.Enums;
using EmployeeLeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Infrastructure.Departments;

public sealed class DepartmentService(ApplicationDbContext dbContext) : IDepartmentService
{
    public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await Project(dbContext.Departments.AsNoTracking()
                .OrderBy(department => department.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<DepartmentDto> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await Project(dbContext.Departments.AsNoTracking().Where(item => item.Id == id))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Department not found.");
    }

    public async Task<DepartmentDto> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (await dbContext.Departments.AnyAsync(
                department => department.Name == name,
                cancellationToken))
        {
            throw new ConflictException("A department with this name already exists.");
        }

        var department = new Department
        {
            Name = name,
            Description = request.Description?.Trim()
        };

        dbContext.Departments.Add(department);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(department.Id, cancellationToken);
    }

    public async Task<DepartmentDto> UpdateAsync(
        Guid id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var department = await dbContext.Departments.SingleOrDefaultAsync(
            item => item.Id == id,
            cancellationToken) ?? throw new NotFoundException("Department not found.");
        var name = request.Name.Trim();

        if (await dbContext.Departments.AnyAsync(
                item => item.Id != id && item.Name == name,
                cancellationToken))
        {
            throw new ConflictException("A department with this name already exists.");
        }

        if (request.ManagerEmployeeId is not null)
        {
            var managerIsValid = await dbContext.Employees.AnyAsync(
                employee => employee.Id == request.ManagerEmployeeId
                    && employee.DepartmentId == id
                    && employee.IsActive
                    && employee.User.Role == UserRole.Manager,
                cancellationToken);

            if (!managerIsValid)
            {
                throw new ConflictException(
                    "The department manager must be an active Manager in this department.");
            }
        }

        department.Name = name;
        department.Description = request.Description?.Trim();
        department.ManagerEmployeeId = request.ManagerEmployeeId;
        department.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await dbContext.Departments.SingleOrDefaultAsync(
            item => item.Id == id,
            cancellationToken) ?? throw new NotFoundException("Department not found.");

        if (await dbContext.Employees.AnyAsync(
                employee => employee.DepartmentId == id && employee.IsActive,
                cancellationToken))
        {
            throw new ConflictException("A department with active employees cannot be deactivated.");
        }

        department.IsActive = false;
        department.ManagerEmployeeId = null;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<DepartmentDto> Project(IQueryable<Department> query)
    {
        return query.Select(department => new DepartmentDto(
            department.Id,
            department.Name,
            department.Description,
            department.ManagerEmployeeId,
            department.ManagerEmployee == null
                ? null
                : department.ManagerEmployee.FirstName + " " + department.ManagerEmployee.LastName,
            department.IsActive,
            department.Employees.Count(employee => employee.IsActive)));
    }
}
