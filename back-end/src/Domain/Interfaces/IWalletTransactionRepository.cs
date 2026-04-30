using ShareFlow.Domain.Entities;

namespace ShareFlow.Domain.Interfaces;

public interface IWalletTransactionRepository
{
    Task AddAsync(WalletTransaction tx, CancellationToken ct = default);
    Task<(IReadOnlyList<WalletTransaction> Items, int Total)> GetPagedByInvestorAsync(
        Guid investorUserId, int page, int pageSize, CancellationToken ct = default);
}
