using ShareFlow.Application.Common;
using ShareFlow.Application.Wallet.DTOs;

namespace ShareFlow.Application.Wallet.Interfaces;

public interface IWalletService
{
    Task<WalletDto> GetByInvestorIdAsync(Guid investorId, CancellationToken ct = default);
    Task<PagedResult<WalletDto>> GetAllAsync(int page, int pageSize, string? keyword = null, CancellationToken ct = default);
    Task AdminCreditAsync(Guid investorId, decimal amount, string remark, Guid adminId, CancellationToken ct = default);
    Task<Guid> RequestWithdrawalAsync(Guid investorId, decimal amount, string? paymentMethod, CancellationToken ct = default);
    Task ApproveWithdrawalAsync(Guid requestId, Guid adminId, CancellationToken ct = default);
    Task CompleteWithdrawalAsync(Guid requestId, Guid adminId, CancellationToken ct = default);
    Task RejectWithdrawalAsync(Guid requestId, Guid adminId, string reason, CancellationToken ct = default);
    Task<PagedResult<WalletTransactionDto>> GetTransactionsAsync(Guid investorId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<WithdrawalRequestDto>> GetWithdrawalRequestsAsync(string? status, int page, int pageSize, string? keyword = null, CancellationToken ct = default);
    Task<PagedResult<WithdrawalRequestDto>> GetMyWithdrawalRequestsAsync(Guid investorId, int page, int pageSize, CancellationToken ct = default);
}
