using System.Text.Json;
using ShareFlow.Application.Common;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Api.Middleware;

public class OverseasBackendAccessRestrictionMiddleware(
    RequestDelegate next,
    IRegionContext regionContext,
    ILogger<OverseasBackendAccessRestrictionMiddleware> logger)
{
    private const string ChinaCountryCode = "CN";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!ShouldRestrict(context))
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";

        logger.LogWarning(
            "Blocked overseas backend access from country {CountryCode}. Path: {Path}",
            GetCountryCode(context),
            context.Request.Path.Value);

        var response = ApiResponse<object>.Fail("当前区域不可访问海外后台", 403);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private bool ShouldRestrict(HttpContext context)
    {
        if (regionContext.Mode != RegionMode.Overseas)
        {
            return false;
        }

        if (!IsBackendPath(context.Request.Path))
        {
            return false;
        }

        var countryCode = GetCountryCode(context);
        return string.Equals(countryCode, ChinaCountryCode, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBackendPath(PathString path)
    {
        return path.StartsWithSegments("/v1/admin", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/v1/sales", StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetCountryCode(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("CF-IPCountry", out var cfCountry)
            && !string.IsNullOrWhiteSpace(cfCountry))
        {
            return cfCountry.ToString();
        }

        if (context.Request.Headers.TryGetValue("X-Country-Code", out var countryCode)
            && !string.IsNullOrWhiteSpace(countryCode))
        {
            return countryCode.ToString();
        }

        return null;
    }
}