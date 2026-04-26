using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShareFlow.Application.Common;
using ShareFlow.Domain.Enums;
using System.Security.Claims;

namespace ShareFlow.Api.Filters;

public class PermissionFilter(string permission) : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedObjectResult(ApiResponse<object>.Fail("未授权。", 401));
            return Task.CompletedTask;
        }

        if (user.IsInRole(UserRole.SuperAdmin.ToString()))
        {
            return Task.CompletedTask;
        }

        var permissions = user.FindAll("permission").Select(static claim => claim.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (permissions.Contains(permission))
        {
            return Task.CompletedTask;
        }

        context.Result = new ObjectResult(ApiResponse<object>.Fail("无权限访问该资源。", 403))
        {
            StatusCode = 403
        };

        return Task.CompletedTask;
    }
}