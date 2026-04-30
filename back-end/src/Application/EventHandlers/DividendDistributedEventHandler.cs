using MediatR;
using ShareFlow.Domain.Events;

namespace ShareFlow.Application.EventHandlers;

/// <summary>
/// 分红分发事件处理器（Epic 6 钱包模块将在此处实现余额入账逻辑）
/// </summary>
public class DividendDistributedEventHandler : INotificationHandler<DividendDistributedEvent>
{
    public Task Handle(DividendDistributedEvent notification, CancellationToken cancellationToken)
    {
        // Epic 6: 触发钱包到账处理
        return Task.CompletedTask;
    }
}
