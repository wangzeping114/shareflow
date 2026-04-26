using ShareFlow.Domain.Common;

namespace ShareFlow.Domain.Entities;

public class BackendRole : Entity<Guid>
{
    private readonly List<User> _users = [];

    private BackendRole()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyList<string> Permissions => _permissions.AsReadOnly();

    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    private List<string> _permissions = [];

    public static BackendRole Create(string name, IEnumerable<string> permissions)
    {
        return new BackendRole
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            _permissions = permissions
                .Where(static permission => !string.IsNullOrWhiteSpace(permission))
                .Select(static permission => permission.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }

    public void Rename(string name)
    {
        Name = name.Trim();
        SetUpdatedAt();
    }

    public void SetPermissions(IEnumerable<string> permissions)
    {
        _permissions = permissions
            .Where(static permission => !string.IsNullOrWhiteSpace(permission))
            .Select(static permission => permission.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        SetUpdatedAt();
    }
}