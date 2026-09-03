using System.Reflection;
using EmployeeLeaveManagement.API.Controllers;
using Microsoft.AspNetCore.Mvc;
#pragma warning disable CA1707

namespace EmployeeLeaveManagement.IntegrationTests;

public sealed class ApiContractTests
{
    [Fact] public void Leave_requests_controller_has_post() => Assert.NotNull(typeof(LeaveRequestsController).GetMethod("Create"));
    [Fact] public void Leave_requests_controller_has_cancel() => Assert.NotNull(typeof(LeaveRequestsController).GetMethod("Cancel"));
    [Fact] public void Leave_requests_controller_has_approve() => Assert.NotNull(typeof(LeaveRequestsController).GetMethod("Approve"));
    [Fact] public void Leave_requests_controller_has_reject() => Assert.NotNull(typeof(LeaveRequestsController).GetMethod("Reject"));
    [Fact] public void Reports_controller_is_authorized() => Assert.NotNull(typeof(ReportsController).GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
}
