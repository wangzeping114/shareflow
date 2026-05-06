namespace ShareFlow.Application.Admin.DTOs;

public class BackendRoleDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public IReadOnlyList<string> Permissions { get; set; } = [];
}

public class CreateBackendRoleRequest
{
    public string Name { get; set; } = string.Empty;

    public List<string> Permissions { get; set; } = [];
}

public class UpdateBackendRolePermissionsRequest
{
    public List<string> Permissions { get; set; } = [];
}

public class AssignBackendRoleRequest
{
    public Guid? BackendRoleId { get; set; }
}

// 内部账号相关
public class InternalUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? BackendRoleId { get; set; }
    public string? BackendRoleName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateInternalUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    /// <summary>BackendCustom 或 Sales</summary>
    public string Role { get; set; } = "BackendCustom";
    public Guid? BackendRoleId { get; set; }
}