using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Entities;

public class Wallet : Entity<Guid>
{
    public Guid InvestorUserId { get; private set; }
    public decimal Balance { get; private set; }
    public decimal FrozenAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;

    private Wallet() { }

    public static Wallet Create(Guid investorUserId, string currency)
    {
        return new Wallet
        {
            Id = Guid.NewGuid(),
            InvestorUserId = investorUserId,
            Balance = 0m,
            FrozenAmount = 0m,
            Currency = currency,
        };
    }

    /// <summary>余额增加（分红到账 / 管理员充值）</summary>
    public WalletTransaction Credit(decimal amount, TransactionType type, string remark, Guid? referenceId = null)
    {
        if (amount <= 0) throw new BusinessException("wallet.invalidAmount");
        var before = Balance;
        Balance += amount;
        SetUpdatedAt();
        return WalletTransaction.Create(Id, InvestorUserId, type, TransactionDirection.In, amount, Currency, before, Balance, remark, referenceId);
    }

    /// <summary>冻结余额（申请提现）</summary>
    public void Freeze(decimal amount)
    {
        if (amount <= 0) throw new BusinessException("wallet.invalidAmount");
        if (amount > Balance) throw new BusinessException("wallet.insufficientBalance");
        Balance -= amount;
        FrozenAmount += amount;
        SetUpdatedAt();
    }

    /// <summary>解冻余额（提现被拒）</summary>
    public WalletTransaction Unfreeze(decimal amount, Guid? referenceId = null)
    {
        var before = Balance;
        FrozenAmount -= amount;
        Balance += amount;
        SetUpdatedAt();
        return WalletTransaction.Create(Id, InvestorUserId, TransactionType.Withdrawal, TransactionDirection.In, amount, Currency, before, Balance, "提现已拒绝，余额退回", referenceId);
    }

    /// <summary>扣减冻结金额（提现完成，线下已打款）</summary>
    public WalletTransaction DeductFrozen(decimal amount, Guid? referenceId = null)
    {
        FrozenAmount -= amount;
        SetUpdatedAt();
        // 余额在 Freeze 时已扣除，此处只记流水
        return WalletTransaction.Create(Id, InvestorUserId, TransactionType.Withdrawal, TransactionDirection.Out, amount, Currency, Balance, Balance, "提现已完成", referenceId);
    }
}
