using MediatR;
using ShareFlow.Domain.Events;

namespace ShareFlow.Application.EventHandlers;

/// <summary>
/// 收益审核通过事件处理器（Epic 5 分红计算引擎将在此基础上实现）
/// </summary>
public class RevenueApprovedEventHandler : INotificationHandler<RevenueApprovedEvent>
{
    public Task Handle(RevenueApprovedEvent notification, CancellationToken cancellationToken)
    {
        // Epic 5: 触发分红计算任务
        return Task.CompletedTask;
    }
}
