using BCrypt.Net;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Entities;

public class User : Entity<Guid>
{
    private readonly List<RefreshToken> _refreshTokens = [];

    private User()
    {
    }

    public string Email { get; private set; } = string.Empty;

    public string Username { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public UserStatus Status { get; private set; }

    public Guid? BackendRoleId { get; private set; }

    public BackendRole? BackendRole { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public static User Create(string username, string email, string password, string displayName, UserRole role)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = username.Trim().ToLowerInvariant(),
            Email = email.Trim().ToLowerInvariant(),
            DisplayName = displayName.Trim(),
            Role = role,
            Status = UserStatus.Active,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };
    }

    public bool VerifyPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }

    public void SetPassword(string password)
    {
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        SetUpdatedAt();
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        SetUpdatedAt();
    }

    public void Disable()
    {
        Status = UserStatus.Disabled;
        SetUpdatedAt();
    }

    public void MarkPendingSetup()
    {
        Status = UserStatus.PendingSetup;
        SetUpdatedAt();
    }

    public void AssignRole(UserRole role, Guid? backendRoleId = null)
    {
        Role = role;
        BackendRoleId = role == UserRole.BackendCustom ? backendRoleId : null;
        SetUpdatedAt();
    }
}