using EmployeeLeaveManagement.Domain.Entities;

namespace EmployeeLeaveManagement.Application.Authentication;

public interface ITokenGenerator
{
    AuthToken Generate(User user);
}
