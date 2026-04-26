using ShareFlow.Application.Common;
using ShareFlow.Domain.Common;
using System.Net;
using System.Text.Json;

namespace ShareFlow.Api.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BusinessException ex)
        {
            logger.LogWarning(ex, "Business exception: {Message}", ex.Message);
            await WriteResponse(context, ex.Code, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteResponse(context, 500, "Internal server error");
        }
    }

    private static async Task WriteResponse(HttpContext context, int code, string message)
    {
        context.Response.StatusCode = code >= 500
            ? (int)HttpStatusCode.InternalServerError
            : (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";
        var response = ApiResponse<object>.Fail(message, code);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
