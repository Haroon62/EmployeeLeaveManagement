using System.Data;
using System.Security.Cryptography;
#pragma warning disable CA1848
using System.Text;
using System.Text.Json;
using EmployeeLeaveManagement.Application.Common.Exceptions;
using EmployeeLeaveManagement.Application.LeaveRequests;
using EmployeeLeaveManagement.Domain.Entities;
using EmployeeLeaveManagement.Domain.Enums;
using EmployeeLeaveManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmployeeLeaveManagement.Infrastructure.LeaveRequests;

public sealed class LeaveRequestService(
    ApplicationDbContext dbContext,
    ILogger<LeaveRequestService> logger) : ILeaveRequestService
{
    public async Task<IReadOnlyList<LeaveRequestStatusDto>> GetAllAsync(
        Guid currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        var query = Scope(dbContext.LeaveRequests.AsNoTracking(), currentUserId, currentUserRole);
        return await Project(query.OrderByDescending(request => request.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<LeaveRequestStatusDto> GetByIdAsync(
        Guid id,
        Guid currentUserId,
        UserRole currentUserRole,
        CancellationToken cancellationToken = default)
    {
        var query = Scope(
            dbContext.LeaveRequests.AsNoTracking().Where(request => request.Id == id),
            currentUserId,
            currentUserRole);
        return await Project(query).SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Leave request not found.");
    }

    public async Task<LeaveRequestStatusDto> CreateAsync(
        Guid currentUserId,
        CreateLeaveRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        idempotencyKey = idempotencyKey.Trim();
        if (idempotencyKey.Length is 0 or > 100)
        {
            throw new ValidationException("A unique Idempotency-Key header is required and must be 1-100 characters.");
        }

        var requestHash = CreateRequestHash(request);
        var existing = await dbContext.IdempotencyRecords
            .Include(record => record.LeaveRequest)
            .SingleOrDefaultAsync(
                record => record.UserId == currentUserId && record.Key == idempotencyKey,
                cancellationToken);
        if (existing is not null)
        {
            if (existing.RequestHash != requestHash || existing.LeaveRequestId is null)
            {
                throw new ConflictException("This Idempotency-Key was already used for a different request.");
            }

            return await GetByIdAsync(
                existing.LeaveRequestId.Value,
                currentUserId,
                UserRole.Employee,
                cancellationToken);
        }

        var employee = await dbContext.Employees.SingleOrDefaultAsync(
            item => item.UserId == currentUserId && item.IsActive,
            cancellationToken) ?? throw new ForbiddenException("An active employee profile is required.");
        var leaveType = await dbContext.LeaveTypes.SingleOrDefaultAsync(
            item => item.Id == request.LeaveTypeId && item.IsActive,
            cancellationToken) ?? throw new NotFoundException("Leave type not found.");

        var workingDays = CountWorkingDays(request.StartDate, request.EndDate);
        var balance = await dbContext.LeaveBalances.SingleOrDefaultAsync(
            item => item.EmployeeId == employee.Id
                && item.LeaveTypeId == leaveType.Id
                && item.Year == request.StartDate.Year,
            cancellationToken) ?? throw new ConflictException("No leave balance exists for the requested year and leave type.");

        if (balance.RemainingDays < workingDays)
        {
            throw new ConflictException("The requested leave exceeds the remaining balance.");
        }

        var overlaps = await dbContext.LeaveRequests.AnyAsync(
            item => item.EmployeeId == employee.Id
                && (item.Status == LeaveRequestStatus.Pending || item.Status == LeaveRequestStatus.Approved)
                && item.StartDate <= request.EndDate
                && item.EndDate >= request.StartDate,
            cancellationToken);
        if (overlaps)
        {
            throw new ConflictException("The requested dates overlap an existing active leave request.");
        }

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveTypeId = leaveType.Id,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            WorkingDays = workingDays,
            Reason = request.Reason.Trim(),
            Status = LeaveRequestStatus.Pending
        };
        leaveRequest.IdempotencyRecord = new IdempotencyRecord
        {
            UserId = currentUserId,
            Key = idempotencyKey,
            RequestHash = requestHash,
            LeaveRequest = leaveRequest
        };
        dbContext.LeaveRequests.Add(leaveRequest);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            logger.LogWarning(exception, "Duplicate leave request submission for user {UserId}", currentUserId);
            var concurrent = await dbContext.IdempotencyRecords
                .SingleOrDefaultAsync(
                    record => record.UserId == currentUserId && record.Key == idempotencyKey,
                    cancellationToken);
            if (concurrent?.RequestHash == requestHash && concurrent.LeaveRequestId is not null)
            {
                return await GetByIdAsync(
                    concurrent.LeaveRequestId.Value,
                    currentUserId,
                    UserRole.Employee,
                    cancellationToken);
            }

            throw new ConflictException("The leave request could not be submitted because the idempotency key was already used.");
        }

        logger.LogInformation("Leave request {RequestId} created by employee {EmployeeId}", leaveRequest.Id, employee.Id);
        return await GetByIdAsync(leaveRequest.Id, currentUserId, UserRole.Employee, cancellationToken);
    }

    public async Task CancelAsync(
        Guid id,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var employeeId = await dbContext.Employees
            .Where(employee => employee.UserId == currentUserId && employee.IsActive)
            .Select(employee => employee.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (employeeId == Guid.Empty)
        {
            throw new ForbiddenException("An active employee profile is required.");
        }

        var affected = await dbContext.LeaveRequests
            .Where(request => request.Id == id
                && request.EmployeeId == employeeId
                && request.Status == LeaveRequestStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(request => request.Status, LeaveRequestStatus.Cancelled)
                .SetProperty(request => request.CancelledAtUtc, DateTime.UtcNow), cancellationToken);
        if (affected == 1)
        {
            logger.LogInformation("Leave request {RequestId} cancelled by employee {EmployeeId}", id, employeeId);
            return;
        }

        var requestExists = await dbContext.LeaveRequests.AnyAsync(request => request.Id == id, cancellationToken);
        throw requestExists
            ? new ConflictException("Only your Pending leave requests can be cancelled.")
            : new NotFoundException("Leave request not found.");
    }

    public async Task ApproveAsync(
        Guid id,
        Guid managerUserId,
        CancellationToken cancellationToken = default)
    {
        var manager = await GetManagerAsync(managerUserId, cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var request = await dbContext.LeaveRequests.SingleOrDefaultAsync(
            item => item.Id == id
                && item.Employee.DepartmentId == manager.DepartmentId
                && item.EmployeeId != manager.Id
                && item.Status == LeaveRequestStatus.Pending,
            cancellationToken) ?? throw new ConflictException(
                "The request is not Pending, is outside your department, or belongs to you.");

        var claimed = await dbContext.LeaveRequests
            .Where(item => item.Id == request.Id && item.Status == LeaveRequestStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.Status, LeaveRequestStatus.Approved)
                .SetProperty(item => item.ReviewedByEmployeeId, manager.Id)
                .SetProperty(item => item.ReviewedAtUtc, DateTime.UtcNow), cancellationToken);
        if (claimed != 1)
        {
            throw new ConflictException("The leave request was already reviewed by another manager.");
        }

        var deducted = await dbContext.LeaveBalances
            .Where(balance => balance.EmployeeId == request.EmployeeId
                && balance.LeaveTypeId == request.LeaveTypeId
                && balance.Year == request.StartDate.Year
                && balance.RemainingDays >= request.WorkingDays)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(balance => balance.UsedDays, balance => balance.UsedDays + request.WorkingDays)
                .SetProperty(balance => balance.RemainingDays, balance => balance.RemainingDays - request.WorkingDays), cancellationToken);
        if (deducted != 1)
        {
            throw new ConflictException("The employee does not have enough remaining leave balance.");
        }

        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Leave request {RequestId} approved by manager {ManagerId}", id, manager.Id);
    }

    public async Task RejectAsync(
        Guid id,
        Guid managerUserId,
        string rejectionReason,
        CancellationToken cancellationToken = default)
    {
        rejectionReason = rejectionReason.Trim();
        if (rejectionReason.Length == 0)
        {
            throw new ValidationException("A rejection reason is required.");
        }

        var manager = await GetManagerAsync(managerUserId, cancellationToken);
        var affected = await dbContext.LeaveRequests
            .Where(item => item.Id == id
                && item.Employee.DepartmentId == manager.DepartmentId
                && item.EmployeeId != manager.Id
                && item.Status == LeaveRequestStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.Status, LeaveRequestStatus.Rejected)
                .SetProperty(item => item.RejectionReason, rejectionReason)
                .SetProperty(item => item.ReviewedByEmployeeId, manager.Id)
                .SetProperty(item => item.ReviewedAtUtc, DateTime.UtcNow), cancellationToken);
        if (affected != 1)
        {
            throw new ConflictException("The request is not Pending, is outside your department, or belongs to you.");
        }

        logger.LogInformation("Leave request {RequestId} rejected by manager {ManagerId}", id, manager.Id);
    }

    private async Task<Employee> GetManagerAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Employees
            .Include(employee => employee.User)
            .SingleOrDefaultAsync(
                employee => employee.UserId == userId
                    && employee.IsActive
                    && employee.User.Role == UserRole.Manager,
                cancellationToken)
            ?? throw new ForbiddenException("An active Manager employee profile is required.");
    }

    private static IQueryable<LeaveRequest> Scope(
        IQueryable<LeaveRequest> query,
        Guid currentUserId,
        UserRole currentUserRole)
    {
        return currentUserRole switch
        {
            UserRole.Admin => query,
            UserRole.Manager => query.Where(request => request.Employee.Department.Employees
                .Any(employee => employee.UserId == currentUserId)),
            _ => query.Where(request => request.Employee.UserId == currentUserId)
        };
    }

    private static IQueryable<LeaveRequestStatusDto> Project(IQueryable<LeaveRequest> query)
    {
        return query.Select(request => new LeaveRequestStatusDto(
            request.Id,
            request.EmployeeId,
            request.Employee.FirstName + " " + request.Employee.LastName,
            request.LeaveTypeId,
            request.LeaveType.Name,
            request.StartDate,
            request.EndDate,
            request.WorkingDays,
            request.Reason,
            request.Status.ToString(),
            request.RejectionReason,
            request.ReviewedByEmployeeId,
            request.ReviewedAtUtc,
            request.CancelledAtUtc,
            request.CreatedAtUtc));
    }

    private static int CountWorkingDays(DateOnly startDate, DateOnly endDate)
    {
        try
        {
            var days = WorkingDayCalculator.Count(startDate, endDate);
            return days > 0
                ? days
                : throw new ValidationException("The requested range contains no working days.");
        }
        catch (ArgumentException exception)
        {
            throw new ValidationException(exception.Message);
        }
    }

    private static string CreateRequestHash(CreateLeaveRequest request)
    {
        var payload = JsonSerializer.Serialize(new
        {
            request.LeaveTypeId,
            request.StartDate,
            request.EndDate,
            Reason = request.Reason.Trim()
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }
}
