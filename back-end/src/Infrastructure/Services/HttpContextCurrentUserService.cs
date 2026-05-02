using Microsoft.AspNetCore.Http;
using ShareFlow.Application.Common.Interfaces;
using System.Security.Claims;

namespace ShareFlow.Infrastructure.Services;

public class HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return value is not null ? Guid.Parse(value) : Guid.Empty;
        }
    }

    public string Role
        => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
}
