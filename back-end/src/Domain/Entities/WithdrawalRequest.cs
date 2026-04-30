using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Entities;

public class WithdrawalRequest : Entity<Guid>
{
    public Guid InvestorUserId { get; private set; }
    public Guid WalletId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public WithdrawalStatus Status { get; private set; }
    /// <summary>客户填写的线下收款信息（银行账号 / 支付宝等）</summary>
    public string? PaymentMethod { get; private set; }
    public string? RejectReason { get; private set; }
    public Guid? ProcessedByUserId { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private WithdrawalRequest() { }

    public static WithdrawalRequest Create(Guid investorUserId, Guid walletId, decimal amount, string currency, string? paymentMethod)
    {
        if (amount <= 0) throw new BusinessException("wallet.invalidAmount");
        return new WithdrawalRequest
        {
            Id = Guid.NewGuid(),
            InvestorUserId = investorUserId,
            WalletId = walletId,
            Amount = amount,
            Currency = currency,
            Status = WithdrawalStatus.Pending,
            PaymentMethod = paymentMethod,
        };
    }

    /// <summary>管理员批准提现申请（余额冻结由 Service 层完成）</summary>
    public void Approve(Guid adminId)
    {
        if (Status != WithdrawalStatus.Pending)
            throw new BusinessException("withdrawal.invalidStatus");
        Status = WithdrawalStatus.Approved;
        ProcessedByUserId = adminId;
        ProcessedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>线下打款完成后标记为已完成</summary>
    public void Complete(Guid adminId)
    {
        if (Status != WithdrawalStatus.Approved)
            throw new BusinessException("withdrawal.invalidStatus");
        Status = WithdrawalStatus.Completed;
        ProcessedByUserId = adminId;
        ProcessedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>拒绝提现申请（余额解冻由 Service 层完成）</summary>
    public void Reject(Guid adminId, string reason)
    {
        if (Status != WithdrawalStatus.Pending && Status != WithdrawalStatus.Approved)
            throw new BusinessException("withdrawal.invalidStatus");
        Status = WithdrawalStatus.Rejected;
        RejectReason = reason;
        ProcessedByUserId = adminId;
        ProcessedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
