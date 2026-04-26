using ShareFlow.Domain.Entities;

namespace ShareFlow.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);

    Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default);

    Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}