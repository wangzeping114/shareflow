using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>获取指定角色的用户列表（用于合同投资人选择器）</summary>
    Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
}