using MediatR;

namespace ShareFlow.Domain.Events;

/// <summary>
/// 分红分发事件：触发 Epic 6 钱包到账处理
/// </summary>
public record DividendDistributedEvent(
    Guid DividendRecordId,
    Guid InvestorUserId,
    decimal DividendAmount,
    string Currency) : INotification;
