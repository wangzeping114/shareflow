using ShareFlow.Application.Auth.DTOs;

namespace ShareFlow.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<TokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}