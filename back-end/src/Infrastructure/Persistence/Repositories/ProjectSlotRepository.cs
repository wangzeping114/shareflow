using Microsoft.EntityFrameworkCore;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Interfaces;
using ShareFlow.Infrastructure.Persistence;

namespace ShareFlow.Infrastructure.Persistence.Repositories;

public class ProjectSlotRepository(AppDbContext db) : IProjectSlotRepository
{
    public async Task<ProjectSlot?> GetByIdAsync(Guid slotId, CancellationToken ct = default)
        => await db.ProjectSlots.AsNoTracking().FirstOrDefaultAsync(x => x.Id == slotId, ct);

    public async Task<IReadOnlyList<ProjectSlot>> GetByIdsAsync(IEnumerable<Guid> slotIds, CancellationToken ct = default)
        => await db.ProjectSlots
            .AsNoTracking()
            .Where(x => slotIds.Contains(x.Id))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProjectSlot>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        => await db.ProjectSlots
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .ToListAsync(ct);

    public async Task AddAsync(ProjectSlot slot, CancellationToken ct = default)
    {
        await db.ProjectSlots.AddAsync(slot, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ProjectSlot slot, CancellationToken ct = default)
    {
        db.ProjectSlots.Update(slot);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateRangeAsync(IEnumerable<ProjectSlot> slots, CancellationToken ct = default)
    {
        db.ProjectSlots.UpdateRange(slots);
        await db.SaveChangesAsync(ct);
    }
}
