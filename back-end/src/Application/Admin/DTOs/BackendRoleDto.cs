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