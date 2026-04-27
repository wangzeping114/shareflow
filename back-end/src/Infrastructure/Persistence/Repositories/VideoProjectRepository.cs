using Microsoft.EntityFrameworkCore;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;
using ShareFlow.Infrastructure.Persistence;
using ShareFlow.Infrastructure.Persistence.Extensions;

namespace ShareFlow.Infrastructure.Persistence.Repositories;

public class VideoProjectRepository(AppDbContext db) : IVideoProjectRepository
{
    public async Task<VideoProject?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.VideoProjects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<VideoProject?> GetByIdWithSlotsAsync(Guid id, CancellationToken ct = default)
    {
        var project = await db.VideoProjects
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (project is null) return null;

        var slots = await db.ProjectSlots
            .Where(s => s.ProjectId == id)
            .ToListAsync(ct);

        project.SetSlots(slots);
        return project;
    }

    public async Task<(IReadOnlyList<VideoProject> Items, int Total)> GetPagedAsync(
        string? titleFilter, ProjectStatus? statusFilter, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.VideoProjects.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(titleFilter))
            query = query.Where(x => x.Title.Contains(titleFilter));

        if (statusFilter.HasValue)
            query = query.Where(x => x.Status == statusFilter.Value);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToPagedAsync(page, pageSize, ct);
    }

    public async Task AddAsync(VideoProject project, CancellationToken ct = default)
    {
        await db.VideoProjects.AddAsync(project, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(VideoProject project, CancellationToken ct = default)
    {
        db.VideoProjects.Update(project);
        await db.SaveChangesAsync(ct);
    }
}
