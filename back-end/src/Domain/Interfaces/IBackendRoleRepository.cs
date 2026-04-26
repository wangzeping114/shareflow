using ShareFlow.Domain.Entities;

namespace ShareFlow.Domain.Interfaces;

public interface IBackendRoleRepository
{
    Task<BackendRole?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BackendRole>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(BackendRole role, CancellationToken cancellationToken = default);

    Task UpdateAsync(BackendRole role, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}