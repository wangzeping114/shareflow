using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShareFlow.Application.Auth;
using ShareFlow.Application.Auth.Interfaces;
using ShareFlow.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ShareFlow.Infrastructure.Services;

public class JwtService(IOptions<JwtSettings> jwtSettings) : IJwtService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public string GenerateAccessToken(User user, IReadOnlyList<string> permissions)
    {
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        claims.AddRange(permissions.Select(static permission => new Claim("permission", permission)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                SecurityAlgorithms.HmacSha256)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            return new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }
}