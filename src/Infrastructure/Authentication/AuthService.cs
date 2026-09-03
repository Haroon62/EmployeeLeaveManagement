using EmployeeLeaveManagement.Application.Authentication;
using EmployeeLeaveManagement.Application.Common.Exceptions;
using EmployeeLeaveManagement.Domain.Entities;
using EmployeeLeaveManagement.Domain.Enums;
using EmployeeLeaveManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmployeeLeaveManagement.Infrastructure.Authentication;

public sealed class AuthService(
    ApplicationDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    ITokenGenerator tokenGenerator,
    ILogger<AuthService> logger) : IAuthService
{
    private static readonly Action<ILogger, string, Exception?> s_logRegistrationConflict =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1001, nameof(s_logRegistrationConflict)),
            "Registration conflict for {Email}");

    private static readonly Action<ILogger, Guid, Exception?> s_logRegistrationSucceeded =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(1002, nameof(s_logRegistrationSucceeded)),
            "Employee account {UserId} registered");

    private static readonly Action<ILogger, string, Exception?> s_logFailedLogin =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1003, nameof(s_logFailedLogin)),
            "Failed login attempt for {Email}");

    private static readonly Action<ILogger, Guid, Exception?> s_logLoginSucceeded =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(1004, nameof(s_logLoginSucceeded)),
            "User {UserId} logged in");

    private static readonly Action<ILogger, Guid, Exception?> s_logLogoutSucceeded =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(1005, nameof(s_logLogoutSucceeded)),
            "User {UserId} logged out");

    private readonly ILogger<AuthService> _logger = logger;

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var normalizedEmployeeNumber = request.EmployeeNumber.Trim().ToUpperInvariant();

        if (await dbContext.Users.AnyAsync(
                user => user.NormalizedEmail == normalizedEmail,
                cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        if (await dbContext.Employees.AnyAsync(
                employee => employee.EmployeeNumber == normalizedEmployeeNumber,
                cancellationToken))
        {
            throw new ConflictException("An employee with this employee number already exists.");
        }

        var department = await dbContext.Departments
            .SingleOrDefaultAsync(
                item => item.Id == request.DepartmentId && item.IsActive,
                cancellationToken)
            ?? throw new NotFoundException("The selected department was not found.");

        var user = new User
        {
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            Role = UserRole.Employee
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        var employee = new Employee
        {
            User = user,
            EmployeeNumber = normalizedEmployeeNumber,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Department = department
        };

        await AddCurrentYearBalancesAsync(employee, cancellationToken);
        dbContext.Employees.Add(employee);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            s_logRegistrationConflict(_logger, request.Email, exception);
            throw new ConflictException("The email or employee number is already registered.");
        }

        s_logRegistrationSucceeded(_logger, user.Id, null);
        return CreateResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await dbContext.Users.SingleOrDefaultAsync(
            item => item.NormalizedEmail == normalizedEmail,
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            s_logFailedLogin(_logger, request.Email, null);
            throw new AuthenticationException("Invalid email or password.");
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            s_logFailedLogin(_logger, request.Email, null);
            throw new AuthenticationException("Invalid email or password.");
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        s_logLoginSucceeded(_logger, user.Id, null);
        return CreateResponse(user);
    }

    public async Task LogoutAsync(
        Guid userId,
        string jwtId,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jwtId))
        {
            throw new AuthenticationException("The access token does not contain a token identifier.");
        }

        if (!await dbContext.RevokedTokens.AnyAsync(token => token.JwtId == jwtId, cancellationToken))
        {
            dbContext.RevokedTokens.Add(new RevokedToken
            {
                UserId = userId,
                JwtId = jwtId,
                ExpiresAtUtc = expiresAtUtc
            });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        s_logLogoutSucceeded(_logger, userId, null);
    }

    private async Task AddCurrentYearBalancesAsync(
        Employee employee,
        CancellationToken cancellationToken)
    {
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
    }

    private AuthResponse CreateResponse(User user)
    {
        var token = tokenGenerator.Generate(user);
        return new AuthResponse(
            user.Id,
            user.Email,
            user.Role.ToString(),
            token.Value,
            token.ExpiresAtUtc);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

}
