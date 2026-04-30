using ShareFlow.Domain.Enums;

namespace ShareFlow.Application.Wallet.DTOs;

public class WalletDto
{
    public Guid Id { get; set; }
    public Guid InvestorUserId { get; set; }
    public string InvestorName { get; set; } = string.Empty;
    public string InvestorEmail { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal FrozenAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class WalletTransactionDto
{
    public long Id { get; set; }
    public TransactionType Type { get; set; }
    public string TypeLabel { get; set; } = string.Empty;
    public TransactionDirection Direction { get; set; }
    public string DirectionLabel { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Remark { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WithdrawalRequestDto
{
    public Guid Id { get; set; }
    public Guid InvestorUserId { get; set; }
    public string InvestorName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public WithdrawalStatus Status { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? RejectReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class AdminCreditRequest
{
    public decimal Amount { get; set; }
    public string Remark { get; set; } = string.Empty;
}

public class RequestWithdrawalRequest
{
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
}

public class RejectWithdrawalRequest
{
    public string Reason { get; set; } = string.Empty;
}
