namespace ShareFlow.Domain.Enums;

public enum TransactionType
{
    Dividend,      // 分红到账
    AdminCredit,   // 管理员充值
    Withdrawal,    // 提现
}

public enum TransactionDirection
{
    In,   // 收入（余额增加）
    Out,  // 支出（余额减少）
}

public enum WithdrawalStatus
{
    Pending,    // 待审核
    Approved,   // 已批准（冻结中，等待线下打款）
    Rejected,   // 已拒绝
    Completed,  // 已完成（线下已打款）
}
