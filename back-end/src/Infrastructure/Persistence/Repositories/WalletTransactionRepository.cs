using Microsoft.EntityFrameworkCore;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Interfaces;
using ShareFlow.Infrastructure.Persistence;

namespace ShareFlow.Infrastructure.Repositories;

public class WalletTransactionRepository(AppDbContext db) : IWalletTransactionRepository
{
    public async Task AddAsync(WalletTransaction tx, CancellationToken ct = default)
    {
        await db.WalletTransactions.AddAsync(tx, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<(IReadOnlyList<WalletTransaction> Items, int Total)> GetPagedByInvestorAsync(
        Guid investorUserId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.WalletTransactions
            .AsNoTracking()
            .Where(t => t.InvestorUserId == investorUserId)
            .OrderByDescending(t => t.Id);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
