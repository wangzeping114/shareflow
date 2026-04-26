using System.Security.Claims;
using ShareFlow.Domain.Entities;

namespace ShareFlow.Application.Auth.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, IReadOnlyList<string> permissions);

    string GenerateRefreshToken();

    ClaimsPrincipal? ValidateToken(string token);
}