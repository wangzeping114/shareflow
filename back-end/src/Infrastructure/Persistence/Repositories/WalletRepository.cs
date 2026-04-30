using Microsoft.EntityFrameworkCore;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Interfaces;
using ShareFlow.Infrastructure.Persistence;

namespace ShareFlow.Infrastructure.Repositories;

public class WalletRepository(AppDbContext db) : IWalletRepository
{
    public Task<Wallet?> GetByInvestorIdAsync(Guid investorUserId, CancellationToken ct = default)
        => db.Wallets.FirstOrDefaultAsync(w => w.InvestorUserId == investorUserId, ct);

    public Task<Wallet?> GetByIdAsync(Guid walletId, CancellationToken ct = default)
        => db.Wallets.FirstOrDefaultAsync(w => w.Id == walletId, ct);

    public async Task<(IReadOnlyList<Wallet> Items, int Total)> GetPagedAsync(int page, int pageSize, IReadOnlyList<Guid>? investorIds = null, CancellationToken ct = default)
    {
        var query = db.Wallets.AsNoTracking();
        if (investorIds is { Count: > 0 })
            query = query.Where(w => investorIds.Contains(w.InvestorUserId));
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(w => w.Balance)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(Wallet wallet, CancellationToken ct = default)
    {
        await db.Wallets.AddAsync(wallet, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Wallet wallet, CancellationToken ct = default)
    {
        db.Wallets.Update(wallet);
        await db.SaveChangesAsync(ct);
    }
}
