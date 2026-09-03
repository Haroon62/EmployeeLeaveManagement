using EmployeeLeaveManagement.Application.Authentication;
using EmployeeLeaveManagement.Application.Departments;
using EmployeeLeaveManagement.Application.Employees;
using EmployeeLeaveManagement.Application.LeaveTypes;
using EmployeeLeaveManagement.Application.LeaveRequests;
using EmployeeLeaveManagement.Application.LeaveBalances;
using EmployeeLeaveManagement.Application.Reports;
using EmployeeLeaveManagement.Domain.Entities;
using EmployeeLeaveManagement.Infrastructure.Authentication;
using EmployeeLeaveManagement.Infrastructure.Departments;
using EmployeeLeaveManagement.Infrastructure.Employees;
using EmployeeLeaveManagement.Infrastructure.LeaveTypes;
using EmployeeLeaveManagement.Infrastructure.LeaveRequests;
using EmployeeLeaveManagement.Infrastructure.LeaveBalances;
using EmployeeLeaveManagement.Infrastructure.Reports;
using EmployeeLeaveManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeLeaveManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = configuration["Jwt:Issuer"] ?? string.Empty;
            options.Audience = configuration["Jwt:Audience"] ?? string.Empty;
            options.SigningKey = configuration["Jwt:SigningKey"] ?? string.Empty;
            options.ExpirationMinutes = int.TryParse(
                configuration["Jwt:ExpirationMinutes"],
                out var expirationMinutes)
                    ? expirationMinutes
                    : 60;
        });
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ILeaveTypeService, LeaveTypeService>();
        services.AddScoped<ILeaveRequestService, LeaveRequestService>();
        services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<DatabaseInitialiser>();

        return services;
    }
}
