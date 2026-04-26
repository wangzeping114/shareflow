using ShareFlow.Domain.Common;

namespace ShareFlow.Domain.Entities;

public class RefreshToken : Entity<Guid>
{
    private RefreshToken()
    {
    }

    public Guid UserId { get; private set; }

    public string Token { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

    public bool IsRevoked { get; private set; }

    public User? User { get; private set; }

    public bool IsActive => !IsRevoked && ExpiresAt > DateTime.UtcNow;

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            IsRevoked = false
        };
    }

    public void Revoke()
    {
        IsRevoked = true;
        SetUpdatedAt();
    }
}