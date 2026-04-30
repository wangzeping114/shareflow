using Microsoft.EntityFrameworkCore;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;
using ShareFlow.Infrastructure.Persistence;

namespace ShareFlow.Infrastructure.Repositories;

public class WithdrawalRequestRepository(AppDbContext db) : IWithdrawalRequestRepository
{
    public Task<WithdrawalRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.WithdrawalRequests.FirstOrDefaultAsync(w => w.Id == id, ct);

    public async Task<(IReadOnlyList<WithdrawalRequest> Items, int Total)> GetPagedAsync(
        WithdrawalStatus? status, int page, int pageSize, IReadOnlyList<Guid>? investorIds = null, CancellationToken ct = default)
    {
        var query = db.WithdrawalRequests.AsNoTracking();
        if (status.HasValue)
            query = query.Where(w => w.Status == status.Value);
        if (investorIds is { Count: > 0 })
            query = query.Where(w => investorIds.Contains(w.InvestorUserId));
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<(IReadOnlyList<WithdrawalRequest> Items, int Total)> GetPagedByInvestorAsync(
        Guid investorUserId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.WithdrawalRequests.AsNoTracking()
            .Where(w => w.InvestorUserId == investorUserId);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(WithdrawalRequest request, CancellationToken ct = default)
    {
        await db.WithdrawalRequests.AddAsync(request, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(WithdrawalRequest request, CancellationToken ct = default)
    {
        db.WithdrawalRequests.Update(request);
        await db.SaveChangesAsync(ct);
    }
}
