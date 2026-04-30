using Microsoft.Extensions.Logging;
using Quartz;
using ShareFlow.Application.Dividend.Interfaces;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Infrastructure.Jobs;

/// <summary>
/// 每天 02:00 UTC 自动补算前一天所有 Approved 但尚未分红的收益
/// </summary>
[DisallowConcurrentExecution]
public class DividendCalculationJob(
    IDividendCalculationService calcService,
    IDividendRepository dividendRepository,
    ILogger<DividendCalculationJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("[DividendCalculationJob] 开始补算未分红收益...");

        var uncalculatedIds = await dividendRepository.GetUncalculatedApprovedRevenueIdsAsync(context.CancellationToken);

        if (uncalculatedIds.Count == 0)
        {
            logger.LogInformation("[DividendCalculationJob] 无需补算，所有已审核收益均已计算分红。");
            return;
        }

        var success = 0;
        foreach (var revenueId in uncalculatedIds)
        {
            try
            {
                await calcService.CalculateForRevenueAsync(revenueId, context.CancellationToken);
                success++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[DividendCalculationJob] 计算收益 {RevenueId} 分红失败", revenueId);
            }
        }

        logger.LogInformation("[DividendCalculationJob] 补算完成：共 {Total} 条，成功 {Success} 条",
            uncalculatedIds.Count, success);
    }
}
