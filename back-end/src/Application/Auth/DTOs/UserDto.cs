using ShareFlow.Domain.Enums;

namespace ShareFlow.Application.Auth.DTOs;

public class UserDto
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public IReadOnlyList<string> Permissions { get; set; } = [];
}