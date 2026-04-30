namespace ShareFlow.Application.Dividend.Interfaces;

public interface IDividendCalculationService
{
    /// <summary>
    /// 对某条已审核收益按所有 Occupied Slots 的 SharePermille 分别计算分红。
    /// 由 RevenueApprovedEventHandler 触发。
    /// </summary>
    Task CalculateForRevenueAsync(Guid revenueId, CancellationToken ct = default);

    /// <summary>批量确认分红记录</summary>
    Task ConfirmBatchAsync(IEnumerable<Guid> dividendIds, Guid operatorId, CancellationToken ct = default);

    /// <summary>批量分发分红（触发钱包到账事件）</summary>
    Task DistributeBatchAsync(IEnumerable<Guid> dividendIds, Guid operatorId, CancellationToken ct = default);
}
