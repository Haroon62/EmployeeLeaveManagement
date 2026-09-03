using EmployeeLeaveManagement.Application.Common.Exceptions;
using EmployeeLeaveManagement.Application.LeaveTypes;
using EmployeeLeaveManagement.Domain.Entities;
using EmployeeLeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Infrastructure.LeaveTypes;

public sealed class LeaveTypeService(ApplicationDbContext dbContext) : ILeaveTypeService
{
    public async Task<IReadOnlyList<LeaveTypeDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await Project(dbContext.LeaveTypes.AsNoTracking()
                .OrderBy(leaveType => leaveType.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<LeaveTypeDto> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await Project(dbContext.LeaveTypes.AsNoTracking().Where(item => item.Id == id))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Leave type not found.");
    }

    public async Task<LeaveTypeDto> CreateAsync(
        CreateLeaveTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (await dbContext.LeaveTypes.AnyAsync(item => item.Name == name, cancellationToken))
        {
            throw new ConflictException("A leave type with this name already exists.");
        }

        var leaveType = new LeaveType
        {
            Name = name,
            Description = request.Description?.Trim(),
            DefaultAnnualDays = request.DefaultAnnualDays
        };

        var employees = await dbContext.Employees
            .Where(employee => employee.IsActive)
            .ToListAsync(cancellationToken);
        foreach (var employee in employees)
        {
            leaveType.LeaveBalances.Add(new LeaveBalance
            {
                Employee = employee,
                Year = DateTime.UtcNow.Year,
                AllocatedDays = request.DefaultAnnualDays,
                RemainingDays = request.DefaultAnnualDays
            });
        }

        dbContext.LeaveTypes.Add(leaveType);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(leaveType.Id, cancellationToken);
    }

    public async Task<LeaveTypeDto> UpdateAsync(
        Guid id,
        UpdateLeaveTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        var leaveType = await dbContext.LeaveTypes.SingleOrDefaultAsync(
            item => item.Id == id,
            cancellationToken) ?? throw new NotFoundException("Leave type not found.");
        var name = request.Name.Trim();

        if (await dbContext.LeaveTypes.AnyAsync(
                item => item.Id != id && item.Name == name,
                cancellationToken))
        {
            throw new ConflictException("A leave type with this name already exists.");
        }

        leaveType.Name = name;
        leaveType.Description = request.Description?.Trim();
        leaveType.DefaultAnnualDays = request.DefaultAnnualDays;
        leaveType.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var leaveType = await dbContext.LeaveTypes.SingleOrDefaultAsync(
            item => item.Id == id,
            cancellationToken) ?? throw new NotFoundException("Leave type not found.");

        leaveType.IsActive = false;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<LeaveTypeDto> Project(IQueryable<LeaveType> query)
    {
        return query.Select(leaveType => new LeaveTypeDto(
            leaveType.Id,
            leaveType.Name,
            leaveType.Description,
            leaveType.DefaultAnnualDays,
            leaveType.IsActive));
    }
}
