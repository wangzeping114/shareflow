using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Interfaces;

public interface IPlatformRevenueRepository
{
    Task<PlatformRevenue?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<PlatformRevenue> Items, int Total)> GetPagedAsync(
        Guid? projectId, RevenueStatus? status, string? platformName,
        int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(PlatformRevenue revenue, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<PlatformRevenue> revenues, CancellationToken ct = default);
    Task UpdateAsync(PlatformRevenue revenue, CancellationToken ct = default);
}
