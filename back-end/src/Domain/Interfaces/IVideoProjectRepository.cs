using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Interfaces;

public interface IVideoProjectRepository
{
    Task<VideoProject?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<VideoProject?> GetByIdWithSlotsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<VideoProject> Items, int Total)> GetPagedAsync(
        string? titleFilter, ProjectStatus? statusFilter, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(VideoProject project, CancellationToken ct = default);
    Task UpdateAsync(VideoProject project, CancellationToken ct = default);
}

public interface IProjectSlotRepository
{
    Task<ProjectSlot?> GetByIdAsync(Guid slotId, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectSlot>> GetByIdsAsync(IEnumerable<Guid> slotIds, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectSlot>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task AddAsync(ProjectSlot slot, CancellationToken ct = default);
    Task UpdateAsync(ProjectSlot slot, CancellationToken ct = default);
    Task UpdateRangeAsync(IEnumerable<ProjectSlot> slots, CancellationToken ct = default);
}
