using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareFlow.Application.Auth.DTOs;
using ShareFlow.Application.Auth.Interfaces;
using ShareFlow.Application.Common;

namespace ShareFlow.Api.Controllers;

[ApiController]
[Route("v1/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<TokenResponse>.Success(response));
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
        return Ok(ApiResponse<TokenResponse>.Success(response));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LogoutAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return Ok(ApiResponse<object>.Success(new { }));
    }
}