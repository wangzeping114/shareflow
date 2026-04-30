using MediatR;
using Microsoft.Extensions.Logging;
using ShareFlow.Application.Wallet.Interfaces;
using ShareFlow.Domain.Events;
using ShareFlow.Domain.Interfaces;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Application.EventHandlers;

/// <summary>
/// 分红分发事件处理器：触发钱包到账
/// </summary>
public class DividendDistributedEventHandler(
    IWalletRepository walletRepository,
    IWalletTransactionRepository txRepository,
    ILogger<DividendDistributedEventHandler> logger) : INotificationHandler<DividendDistributedEvent>
{
    public async Task Handle(DividendDistributedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var wallet = await walletRepository.GetByInvestorIdAsync(notification.InvestorUserId, cancellationToken);
            if (wallet is null)
            {
                // 自动初始化钱包
                wallet = Domain.Entities.Wallet.Create(notification.InvestorUserId, notification.Currency);
                await walletRepository.AddAsync(wallet, cancellationToken);
            }

            var tx = wallet.Credit(notification.DividendAmount, TransactionType.Dividend,
                "分红到账", notification.DividendRecordId);
            await walletRepository.UpdateAsync(wallet, cancellationToken);
            await txRepository.AddAsync(tx, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DividendDistributedEventHandler failed for DividendRecord {Id}", notification.DividendRecordId);
        }
    }
}
