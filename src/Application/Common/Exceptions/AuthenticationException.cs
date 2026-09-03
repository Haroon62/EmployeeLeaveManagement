namespace EmployeeLeaveManagement.Application.Common.Exceptions;

public sealed class AuthenticationException(string message) : Exception(message);
