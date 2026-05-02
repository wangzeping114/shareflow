using Microsoft.EntityFrameworkCore;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;
using ShareFlow.Infrastructure.Persistence;

namespace ShareFlow.Infrastructure.Persistence.Repositories;

public class LeadRepository(AppDbContext db) : ILeadRepository
{
    public async Task<Lead?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Leads.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<(IReadOnlyList<Lead> Items, int Total)> GetPagedAsync(
        Guid salesOwnerId,
        LeadStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = db.Leads.Where(x => x.SalesOwnerId == salesOwnerId);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<int> CountByStatusAsync(Guid salesOwnerId, LeadStatus? status = null, CancellationToken ct = default)
    {
        var query = db.Leads.Where(x => x.SalesOwnerId == salesOwnerId);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        return await query.CountAsync(ct);
    }

    public async Task AddAsync(Lead lead, CancellationToken ct = default)
    {
        await db.Leads.AddAsync(lead, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Lead lead, CancellationToken ct = default)
    {
        db.Leads.Update(lead);
        await db.SaveChangesAsync(ct);
    }
}
