using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Application.Common;

public static class ClientTextLocalizer
{
    public static bool UseEnglish(IRegionContext regionContext)
        => regionContext.DefaultLocale.StartsWith("en", StringComparison.OrdinalIgnoreCase);

    public static string GetDividendStatusLabel(DividendStatus status, bool useEnglish) => (useEnglish, status) switch
    {
        (true, DividendStatus.Calculated) => "Pending Confirmation",
        (true, DividendStatus.Confirmed) => "Pending Distribution",
        (true, DividendStatus.Distributed) => "Credited",
        (false, DividendStatus.Calculated) => "待确认",
        (false, DividendStatus.Confirmed) => "待发放",
        (false, DividendStatus.Distributed) => "已到账",
        _ => status.ToString(),
    };

    public static string GetContractStatusLabel(ContractStatus status, bool useEnglish) => (useEnglish, status) switch
    {
        (true, ContractStatus.Draft) => "Draft",
        (true, ContractStatus.Sent) => "Pending Signature",
        (true, ContractStatus.Signed) => "Signed",
        (true, ContractStatus.Executed) => "Active",
        (true, ContractStatus.Expired) => "Expired",
        (true, ContractStatus.PendingRenew) => "Pending Renewal",
        (true, ContractStatus.Renewing) => "Renewing",
        (true, ContractStatus.Superseded) => "Superseded",
        (false, ContractStatus.Draft) => "草稿",
        (false, ContractStatus.Sent) => "待签署",
        (false, ContractStatus.Signed) => "已签署",
        (false, ContractStatus.Executed) => "生效中",
        (false, ContractStatus.Expired) => "已到期",
        (false, ContractStatus.PendingRenew) => "等待续签",
        (false, ContractStatus.Renewing) => "续签中",
        (false, ContractStatus.Superseded) => "已取代",
        _ => status.ToString(),
    };

    public static string GetTransactionTypeLabel(TransactionType type, bool useEnglish) => (useEnglish, type) switch
    {
        (true, TransactionType.Dividend) => "Dividend",
        (true, TransactionType.AdminCredit) => "Admin Credit",
        (true, TransactionType.Withdrawal) => "Withdrawal",
        (false, TransactionType.Dividend) => "分红到账",
        (false, TransactionType.AdminCredit) => "管理员充值",
        (false, TransactionType.Withdrawal) => "提现",
        _ => type.ToString(),
    };

    public static string GetWithdrawalStatusLabel(WithdrawalStatus status, bool useEnglish) => (useEnglish, status) switch
    {
        (true, WithdrawalStatus.Pending) => "Pending Review",
        (true, WithdrawalStatus.Approved) => "Approved",
        (true, WithdrawalStatus.Rejected) => "Rejected",
        (true, WithdrawalStatus.Completed) => "Completed",
        (false, WithdrawalStatus.Pending) => "待审核",
        (false, WithdrawalStatus.Approved) => "已批准",
        (false, WithdrawalStatus.Rejected) => "已拒绝",
        (false, WithdrawalStatus.Completed) => "已完成",
        _ => status.ToString(),
    };

    public static string LocalizeWalletRemark(string remark, bool useEnglish)
    {
        if (string.IsNullOrWhiteSpace(remark))
        {
            return remark;
        }

        return (useEnglish, remark) switch
        {
            (true, "分红到账") => "Dividend credited",
            (true, "提现已完成") => "Withdrawal completed",
            (true, "提现已拒绝，余额退回") => "Withdrawal rejected, balance returned",
            (false, "Dividend credited") => "分红到账",
            (false, "Withdrawal completed") => "提现已完成",
            (false, "Withdrawal rejected, balance returned") => "提现已拒绝，余额退回",
            _ => remark,
        };
    }
}
