using Microsoft.EntityFrameworkCore;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;
using ShareFlow.Infrastructure.Persistence;

namespace ShareFlow.Infrastructure.Persistence.Repositories;

public class DividendRepository(AppDbContext db) : IDividendRepository
{
    public async Task<DividendRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.DividendRecords.FindAsync([id], ct);

    public async Task<IReadOnlyList<DividendRecord>> GetByRevenueIdAsync(Guid revenueId, CancellationToken ct = default)
        => await db.DividendRecords
            .AsNoTracking()
            .Where(x => x.PlatformRevenueId == revenueId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<DividendRecord>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        => await db.DividendRecords
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<DividendRecord> Items, int Total)> GetPagedAsync(
        IReadOnlyList<Guid>? projectIds,
        Guid? investorUserId,
        IReadOnlyList<DividendStatus>? statuses,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.DividendRecords
            .Include(x => x.Project)
            .Include(x => x.InvestorUser)
            .Include(x => x.PlatformRevenue)
            .AsNoTracking()
            .AsQueryable();

        if (projectIds is { Count: > 0 })
            query = query.Where(x => projectIds.Contains(x.ProjectId));

        if (investorUserId.HasValue)
            query = query.Where(x => x.InvestorUserId == investorUserId.Value);

        if (statuses is { Count: > 0 })
            query = query.Where(x => statuses.Contains(x.Status));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.CalculatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<DividendProjectSummary> GetProjectSummaryAsync(Guid projectId, CancellationToken ct = default)
    {
        var records = await db.DividendRecords
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .ToListAsync(ct);

        var currency = records.FirstOrDefault()?.Currency ?? string.Empty;

        return new DividendProjectSummary(
            ProjectId: projectId,
            TotalCalculated: records.Where(x => x.Status == DividendStatus.Calculated).Sum(x => x.DividendAmount),
            TotalConfirmed: records.Where(x => x.Status == DividendStatus.Confirmed).Sum(x => x.DividendAmount),
            TotalDistributed: records.Where(x => x.Status == DividendStatus.Distributed).Sum(x => x.DividendAmount),
            RecordCount: records.Count,
            Currency: currency);
    }

    public async Task<IReadOnlyList<Guid>> GetUncalculatedApprovedRevenueIdsAsync(CancellationToken ct = default)
    {
        // 获取所有 Approved 收益的 ID
        var approvedRevenueIds = await db.PlatformRevenues
            .AsNoTracking()
            .Where(x => x.Status == ShareFlow.Domain.Enums.RevenueStatus.Approved)
            .Select(x => x.Id)
            .ToListAsync(ct);

        // 已有分红记录的收益 ID
        var calculatedRevenueIds = await db.DividendRecords
            .AsNoTracking()
            .Select(x => x.PlatformRevenueId)
            .Distinct()
            .ToListAsync(ct);

        return approvedRevenueIds
            .Except(calculatedRevenueIds)
            .ToList();
    }

    public async Task AddRangeAsync(IEnumerable<DividendRecord> records, CancellationToken ct = default)
    {
        await db.DividendRecords.AddRangeAsync(records, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateRangeAsync(IEnumerable<DividendRecord> records, CancellationToken ct = default)
    {
        db.DividendRecords.UpdateRange(records);
        await db.SaveChangesAsync(ct);
    }
}
