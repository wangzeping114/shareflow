using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Entities;

public class WalletTransaction : Entity<long>
{
    public Guid WalletId { get; private set; }
    public Guid InvestorUserId { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionDirection Direction { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public decimal BalanceBefore { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public string Remark { get; private set; } = string.Empty;
    /// <summary>关联业务 ID（DividendRecordId 或 WithdrawalRequestId）</summary>
    public Guid? ReferenceId { get; private set; }

    private WalletTransaction() { }

    public static WalletTransaction Create(
        Guid walletId,
        Guid investorUserId,
        TransactionType type,
        TransactionDirection direction,
        decimal amount,
        string currency,
        decimal balanceBefore,
        decimal balanceAfter,
        string remark,
        Guid? referenceId = null)
    {
        return new WalletTransaction
        {
            WalletId = walletId,
            InvestorUserId = investorUserId,
            Type = type,
            Direction = direction,
            Amount = amount,
            Currency = currency,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            Remark = remark,
            ReferenceId = referenceId,
        };
    }
}
