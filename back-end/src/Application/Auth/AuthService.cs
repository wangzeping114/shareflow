using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ShareFlow.Application.Auth.DTOs;
using ShareFlow.Application.Auth.Interfaces;
using ShareFlow.Application.Common.Interfaces;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Constants;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ShareFlow.Application.Auth;

public class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtService jwtService,
    IMapper mapper,
    IOptions<JwtSettings> jwtSettings,
    IRedisService redisService,
    IHttpContextAccessor httpContextAccessor) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null || !user.VerifyPassword(request.Password))
        {
            throw new BusinessException("用户名或密码错误。", 400);
        }

        EnsureUserCanSignIn(user);

        return await IssueTokenPairAsync(user, cancellationToken);
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
        if (tokenEntity is null || !tokenEntity.IsActive)
        {
            throw new BusinessException("刷新令牌无效或已过期。", 400);
        }

        var user = await userRepository.GetByIdAsync(tokenEntity.UserId, cancellationToken)
            ?? throw new BusinessException("用户不存在。", 404);

        EnsureUserCanSignIn(user);
        tokenEntity.Revoke();
        await refreshTokenRepository.UpdateAsync(tokenEntity, cancellationToken);

        return await IssueTokenPairAsync(user, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
        if (tokenEntity is not null && tokenEntity.IsActive)
        {
            await refreshTokenRepository.RevokeAllByUserIdAsync(tokenEntity.UserId, cancellationToken);
        }

        var accessToken = ExtractBearerToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return;
        }

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        var jti = jwt.Claims.FirstOrDefault(static claim => claim.Type == JwtRegisteredClaimNames.Jti)?.Value;
        var expiresAt = jwt.ValidTo;
        if (string.IsNullOrWhiteSpace(jti))
        {
            return;
        }

        var ttl = expiresAt - DateTime.UtcNow;
        if (ttl > TimeSpan.Zero)
        {
            await redisService.SetStringAsync($"token:blacklist:{jti}", "1", ttl, cancellationToken);
        }
    }

    private async Task<TokenResponse> IssueTokenPairAsync(User user, CancellationToken cancellationToken)
    {
        var permissions = await ResolvePermissionsAsync(user, cancellationToken);
        var accessToken = jwtService.GenerateAccessToken(user, permissions);
        var refreshToken = jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes);

        await refreshTokenRepository.AddAsync(
            RefreshToken.Create(user.Id, refreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)),
            cancellationToken);

        var userDto = mapper.Map<UserDto>(user);
        userDto.Permissions = permissions;

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = userDto
        };
    }

    private async Task<IReadOnlyList<string>> ResolvePermissionsAsync(User user, CancellationToken cancellationToken)
    {
        if (user.Role == UserRole.SuperAdmin)
        {
            return
            [
                Permissions.ProjectWrite,
                Permissions.ProjectRead,
                Permissions.RevenueWrite,
                Permissions.RevenueVerify,
                Permissions.DividendWrite,
                Permissions.DividendRead,
                Permissions.ContractRead,
                Permissions.ContractRenew,
                Permissions.WithdrawalApprove,
                Permissions.UserManage,
                Permissions.RoleManage,
                Permissions.ReportRead
            ];
        }

        if (user.Role == UserRole.BackendCustom)
        {
            return await userRepository.GetPermissionsAsync(user.Id, cancellationToken);
        }

        return user.Role switch
        {
            UserRole.Sales => [Permissions.ProjectRead, Permissions.ContractRead],
            UserRole.Client => [Permissions.DividendRead],
            _ => []
        };
    }

    private static void EnsureUserCanSignIn(User user)
    {
        if (user.Status == UserStatus.Disabled)
        {
            throw new BusinessException("账号已被禁用。", 403);
        }

        if (user.Status == UserStatus.PendingSetup)
        {
            throw new BusinessException("账号尚未完成初始化。", 403);
        }
    }

    private string? ExtractBearerToken()
    {
        var header = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return header[7..].Trim();
    }
}