using ShareFlow.Domain.Entities;

namespace ShareFlow.Domain.Interfaces;

public interface IWalletRepository
{
    Task<Wallet?> GetByInvestorIdAsync(Guid investorUserId, CancellationToken ct = default);
    Task<Wallet?> GetByIdAsync(Guid walletId, CancellationToken ct = default);
    Task<(IReadOnlyList<Wallet> Items, int Total)> GetPagedAsync(int page, int pageSize, IReadOnlyList<Guid>? investorIds = null, CancellationToken ct = default);
    Task AddAsync(Wallet wallet, CancellationToken ct = default);
    Task UpdateAsync(Wallet wallet, CancellationToken ct = default);
}
