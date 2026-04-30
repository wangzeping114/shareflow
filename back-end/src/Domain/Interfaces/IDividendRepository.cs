using ShareFlow.Domain.Common;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Interfaces;

public interface IDividendRepository
{
    Task<DividendRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<DividendRecord>> GetByRevenueIdAsync(Guid revenueId, CancellationToken ct = default);

    Task<IReadOnlyList<DividendRecord>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);

    Task<(IReadOnlyList<DividendRecord> Items, int Total)> GetPagedAsync(
        IReadOnlyList<Guid>? projectIds,
        Guid? investorUserId,
        IReadOnlyList<DividendStatus>? statuses,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>返回项目级别的分红汇总（各状态合计金额）</summary>
    Task<DividendProjectSummary> GetProjectSummaryAsync(Guid projectId, CancellationToken ct = default);

    /// <summary>查询尚未计算分红的 Approved 收益 ID 列表（用于 Quartz 补算）</summary>
    Task<IReadOnlyList<Guid>> GetUncalculatedApprovedRevenueIdsAsync(CancellationToken ct = default);

    Task AddRangeAsync(IEnumerable<DividendRecord> records, CancellationToken ct = default);
    Task UpdateRangeAsync(IEnumerable<DividendRecord> records, CancellationToken ct = default);
}

public record DividendProjectSummary(
    Guid ProjectId,
    decimal TotalCalculated,
    decimal TotalConfirmed,
    decimal TotalDistributed,
    int RecordCount,
    string Currency);
