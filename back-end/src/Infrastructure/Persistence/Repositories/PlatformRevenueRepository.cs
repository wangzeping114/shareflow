using Microsoft.EntityFrameworkCore;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;
using ShareFlow.Infrastructure.Persistence;

namespace ShareFlow.Infrastructure.Repositories;

public class PlatformRevenueRepository(AppDbContext db) : IPlatformRevenueRepository
{
    public async Task<PlatformRevenue?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.PlatformRevenues.FindAsync([id], ct);

    public async Task<(IReadOnlyList<PlatformRevenue> Items, int Total)> GetPagedAsync(
        Guid? projectId, RevenueStatus? status, string? platformName,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.PlatformRevenues
            .Include(r => r.Project)
            .AsNoTracking()
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(r => r.ProjectId == projectId.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(platformName))
            query = query.Where(r => EF.Functions.ILike(r.PlatformName, $"%{platformName.Trim()}%"));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.RevenueDate)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(PlatformRevenue revenue, CancellationToken ct = default)
    {
        await db.PlatformRevenues.AddAsync(revenue, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<PlatformRevenue> revenues, CancellationToken ct = default)
    {
        await db.PlatformRevenues.AddRangeAsync(revenues, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PlatformRevenue revenue, CancellationToken ct = default)
    {
        db.PlatformRevenues.Update(revenue);
        await db.SaveChangesAsync(ct);
    }
}
