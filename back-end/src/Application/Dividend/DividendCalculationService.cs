using MapsterMapper;
using MediatR;
using ShareFlow.Application.Common;
using ShareFlow.Application.Dividend.DTOs;
using ShareFlow.Application.Dividend.Interfaces;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Events;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Application.Dividend;

public class DividendCalculationService(
    IDividendRepository dividendRepository,
    IPlatformRevenueRepository revenueRepository,
    IProjectSlotRepository slotRepository,
    IPublisher mediator) : IDividendCalculationService
{
    public async Task CalculateForRevenueAsync(Guid revenueId, CancellationToken ct = default)
    {
        var revenue = await revenueRepository.GetByIdAsync(revenueId, ct);
        if (revenue is null || revenue.Status != RevenueStatus.Approved)
            return;

        // 查询该项目所有已占用的槽位
        var slots = await slotRepository.GetByProjectIdAsync(revenue.ProjectId, ct);
        var occupiedSlots = slots.Where(s => s.Status == SlotStatus.Occupied && s.ClientUserId.HasValue).ToList();

        if (occupiedSlots.Count == 0)
            return;

        // 已存在分红记录的 SlotId（防止重复计算同一槽位）
        var existing = await dividendRepository.GetByRevenueIdAsync(revenueId, ct);
        var calculatedSlotIds = existing.Select(r => r.SlotId).ToHashSet();

        var newRecords = occupiedSlots
            .Where(slot => !calculatedSlotIds.Contains(slot.Id))
            .Select(slot => DividendRecord.Calculate(
                projectId: revenue.ProjectId,
                slotId: slot.Id,
                investorUserId: slot.ClientUserId!.Value,
                revenueId: revenueId,
                revenueAmount: revenue.Amount,
                sharePermille: slot.SharePermille,
                currency: revenue.Currency))
            .ToList();

        if (newRecords.Count > 0)
            await dividendRepository.AddRangeAsync(newRecords, ct);
    }

    public async Task ConfirmBatchAsync(IEnumerable<Guid> dividendIds, Guid operatorId, CancellationToken ct = default)
    {
        var records = await dividendRepository.GetByIdsAsync(dividendIds, ct);

        var toUpdate = new List<DividendRecord>();
        foreach (var record in records)
        {
            if (record.Status == DividendStatus.Calculated)
            {
                record.Confirm();
                toUpdate.Add(record);
            }
        }

        if (toUpdate.Count > 0)
            await dividendRepository.UpdateRangeAsync(toUpdate, ct);
    }

    public async Task DistributeBatchAsync(IEnumerable<Guid> dividendIds, Guid operatorId, CancellationToken ct = default)
    {
        var records = await dividendRepository.GetByIdsAsync(dividendIds, ct);

        var toUpdate = new List<DividendRecord>();
        foreach (var record in records)
        {
            if (record.Status == DividendStatus.Confirmed)
            {
                record.MarkDistributed();
                toUpdate.Add(record);
            }
        }

        if (toUpdate.Count > 0)
        {
            await dividendRepository.UpdateRangeAsync(toUpdate, ct);

            // 发布领域事件，Epic 6 钱包模块监听
            foreach (var record in toUpdate)
            {
                await mediator.Publish(
                    new DividendDistributedEvent(record.Id, record.InvestorUserId, record.DividendAmount, record.Currency), ct);
            }
        }
    }
}
