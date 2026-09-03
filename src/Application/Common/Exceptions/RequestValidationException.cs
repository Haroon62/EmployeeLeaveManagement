namespace EmployeeLeaveManagement.Application.Common.Exceptions;

public sealed class RequestValidationException(string message) : Exception(message);
