using MediatR;
using Microsoft.Extensions.Logging;
using ShareFlow.Application.Dividend.Interfaces;
using ShareFlow.Domain.Events;

namespace ShareFlow.Application.EventHandlers;

/// <summary>
/// 收益审核通过后自动触发分红计算。
/// 分红计算失败不应影响审核结果，异常在此隔离并记录日志。
/// </summary>
public class RevenueApprovedEventHandler(
    IDividendCalculationService calcService,
    ILogger<RevenueApprovedEventHandler> logger)
    : INotificationHandler<RevenueApprovedEvent>
{
    public async Task Handle(RevenueApprovedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await calcService.CalculateForRevenueAsync(notification.RevenueId, cancellationToken);
        }
        catch (Exception ex)
        {
            // 分红计算失败不回滚审核结果，记录日志后由 Quartz 定时任务补算
            logger.LogError(ex,
                "[RevenueApprovedEventHandler] 收益 {RevenueId} 分红自动计算失败，将由定时任务补算",
                notification.RevenueId);
        }
    }
}
