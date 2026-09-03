namespace EmployeeLeaveManagement.API.Logging;

public static partial class ApiLogMessages
{
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Warning,
        Message = "Unauthorized request to {Path} from {RemoteIp}")]
    public static partial void UnauthorizedRequest(
        ILogger logger,
        string path,
        string? remoteIp);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Warning,
        Message = "Forbidden request by user {UserId} to {Path}")]
    public static partial void ForbiddenRequest(
        ILogger logger,
        string? userId,
        string path);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Error,
        Message = "Unhandled request error")]
    public static partial void UnhandledRequestError(ILogger logger, Exception exception);
}
