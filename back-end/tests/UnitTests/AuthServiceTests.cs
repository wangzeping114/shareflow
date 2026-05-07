using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ShareFlow.Application.Auth;
using ShareFlow.Application.Auth.DTOs;
using ShareFlow.Application.Auth.Interfaces;
using ShareFlow.Application.Common.Interfaces;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokenResponse()
    {
        var user = User.Create("admin", "admin@shareflow.local", "Admin@123456", "Admin", UserRole.SuperAdmin);
        var userRepository = new FakeUserRepository(user);
        var refreshTokenRepository = new FakeRefreshTokenRepository();
        var authService = CreateAuthService(userRepository, refreshTokenRepository);

        var result = await authService.LoginAsync(new LoginRequest
        {
            Username = "admin",
            Password = "Admin@123456"
        });

        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.Equal(UserRole.SuperAdmin, result.User.Role);
    }

    [Fact]
    public async Task LoginAsync_DisabledUser_ThrowsBusinessException()
    {
        var user = User.Create("disabled", "disabled@shareflow.local", "Admin@123456", "Disabled", UserRole.Client);
        user.Disable();

        var authService = CreateAuthService(new FakeUserRepository(user), new FakeRefreshTokenRepository());

        await Assert.ThrowsAsync<BusinessException>(() => authService.LoginAsync(new LoginRequest
        {
            Username = "disabled",
            Password = "Admin@123456"
        }));
    }

    [Fact]
    public async Task RefreshTokenAsync_RevokedToken_ThrowsBusinessException()
    {
        var refreshTokenRepository = new FakeRefreshTokenRepository();
        refreshTokenRepository.Token = RefreshToken.Create(Guid.NewGuid(), "revoked-token", DateTime.UtcNow.AddDays(1));
        refreshTokenRepository.Token.Revoke();

        var authService = CreateAuthService(new FakeUserRepository(), refreshTokenRepository);

        await Assert.ThrowsAsync<BusinessException>(() => authService.RefreshTokenAsync("revoked-token"));
    }

    private static AuthService CreateAuthService(FakeUserRepository userRepository, FakeRefreshTokenRepository refreshTokenRepository)
    {
        return new AuthService(
            userRepository,
            refreshTokenRepository,
            new FakeJwtService(),
            CreateMapper(),
            Options.Create(new JwtSettings
            {
                SecretKey = "CHANGE_THIS_SECRET_KEY_MIN_32_CHARS!!",
                Issuer = "ShareFlow",
                Audience = "ShareFlowClients",
                AccessTokenExpiryMinutes = 120,
                RefreshTokenExpiryDays = 7
            }),
            new FakeRedisService(),
            new FakeHttpContextAccessor());
    }

    private static IMapper CreateMapper()
    {
        var config = new TypeAdapterConfig();
        new AuthMappingConfig().Register(config);
        return new Mapper(config);
    }

    private sealed class FakeJwtService : IJwtService
    {
        public string GenerateAccessToken(User user, IReadOnlyList<string> permissions) => "access-token";

        public string GenerateRefreshToken() => "refresh-token";

        public System.Security.Claims.ClaimsPrincipal? ValidateToken(string token) => null;
    }

    private sealed class FakeRedisService : IRedisService
    {
        public Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);

        public Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult(false);

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task SetStringAsync(string key, string value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }

    private sealed class FakeUserRepository(params User[] users) : IUserRepository
    {
        private readonly List<User> _users = users.ToList();

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_users.FirstOrDefault(x => x.Id == id));

        public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) => Task.FromResult(_users.FirstOrDefault(x => x.Username == username));

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(_users.FirstOrDefault(x => x.Email == email));

        public Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default) => Task.FromResult(_users.Any(x => x.Username == username));

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(_users.Any(x => x.Email == email));

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<string>>([]);

        public Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<User>>(_users.Where(u => u.Role == role).ToList());

        public Task<IReadOnlyList<User>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<User>>(_users.Where(u => ids.Contains(u.Id)).ToList());

        public Task<IReadOnlyList<Guid>> SearchIdsByKeywordAsync(string keyword, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Guid>>([]);

        public Task<IReadOnlyList<User>> GetInternalUsersAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<User>>([]);
    }

    private sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        public RefreshToken? Token { get; set; }

        public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
            => Task.FromResult(Token is not null && Token.Token == token ? Token : null);

        public Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            Token = token;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            Token = token;
            return Task.CompletedTask;
        }

        public Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (Token?.UserId == userId)
            {
                Token.Revoke();
            }

            return Task.CompletedTask;
        }
    }
}