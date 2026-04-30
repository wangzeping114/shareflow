using Mapster;
using ShareFlow.Application.Wallet.DTOs;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Application.Wallet;

public class WalletMappingConfig : IRegister
{
    private static string GetTypeLabel(TransactionType t) => t switch
    {
        TransactionType.Dividend => "分红到账",
        TransactionType.AdminCredit => "管理员充值",
        TransactionType.Withdrawal => "提现",
        _ => t.ToString(),
    };

    private static string GetStatusLabel(WithdrawalStatus s) => s switch
    {
        WithdrawalStatus.Pending => "待审核",
        WithdrawalStatus.Approved => "已批准",
        WithdrawalStatus.Rejected => "已拒绝",
        WithdrawalStatus.Completed => "已完成",
        _ => s.ToString(),
    };

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<WalletTransaction, WalletTransactionDto>()
            .Map(dest => dest.TypeLabel, src => GetTypeLabel(src.Type))
            .Map(dest => dest.DirectionLabel, src => src.Direction == TransactionDirection.In ? "+" : "-");

        config.NewConfig<WithdrawalRequest, WithdrawalRequestDto>()
            .Map(dest => dest.StatusLabel, src => GetStatusLabel(src.Status))
            .Map(dest => dest.InvestorName, src => (string?)null);
    }
}
