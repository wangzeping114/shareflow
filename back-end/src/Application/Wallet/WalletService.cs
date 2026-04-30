using Mapster;
using MapsterMapper;
using ShareFlow.Application.Common;
using ShareFlow.Application.Wallet.DTOs;
using ShareFlow.Application.Wallet.Interfaces;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Application.Wallet;

public class WalletService(
    IWalletRepository walletRepository,
    IWalletTransactionRepository txRepository,
    IWithdrawalRequestRepository withdrawalRepository,
    IUserRepository userRepository,
    IMapper mapper) : IWalletService
{
    public async Task<WalletDto> GetByInvestorIdAsync(Guid investorId, CancellationToken ct = default)
    {
        var wallet = await walletRepository.GetByInvestorIdAsync(investorId, ct)
            ?? throw new BusinessException("wallet.notFound");
        var user = await userRepository.GetByIdAsync(investorId, ct);
        var dto = wallet.Adapt<WalletDto>();
        dto.InvestorName = user?.DisplayName ?? string.Empty;
        dto.InvestorEmail = user?.Email ?? string.Empty;
        return dto;
    }

    public async Task<PagedResult<WalletDto>> GetAllAsync(int page, int pageSize, string? keyword = null, CancellationToken ct = default)
    {
        IReadOnlyList<Guid>? investorIds = null;
        if (!string.IsNullOrWhiteSpace(keyword))
            investorIds = await userRepository.SearchIdsByKeywordAsync(keyword, ct);
        var (items, total) = await walletRepository.GetPagedAsync(page, pageSize, investorIds, ct);
        var userIds = items.Select(w => w.InvestorUserId).ToList();
        var users = await userRepository.GetByIdsAsync(userIds, ct);
        var userMap = users.ToDictionary(u => u.Id);

        var dtos = items.Select(w =>
        {
            var dto = w.Adapt<WalletDto>();
            if (userMap.TryGetValue(w.InvestorUserId, out var user))
            {
                dto.InvestorName = user.DisplayName;
                dto.InvestorEmail = user.Email;
            }
            return dto;
        }).ToList();

        return new PagedResult<WalletDto> { Items = dtos, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task AdminCreditAsync(Guid investorId, decimal amount, string remark, Guid adminId, CancellationToken ct = default)
    {
        var wallet = await GetOrCreateWalletAsync(investorId, ct);
        var tx = wallet.Credit(amount, TransactionType.AdminCredit, remark);
        await walletRepository.UpdateAsync(wallet, ct);
        await txRepository.AddAsync(tx, ct);
    }

    public async Task<Guid> RequestWithdrawalAsync(Guid investorId, decimal amount, string? paymentMethod, CancellationToken ct = default)
    {
        var wallet = await walletRepository.GetByInvestorIdAsync(investorId, ct)
            ?? throw new BusinessException("wallet.notFound");
        wallet.Freeze(amount);
        var request = WithdrawalRequest.Create(investorId, wallet.Id, amount, wallet.Currency, paymentMethod);
        await walletRepository.UpdateAsync(wallet, ct);
        await withdrawalRepository.AddAsync(request, ct);
        return request.Id;
    }

    public async Task ApproveWithdrawalAsync(Guid requestId, Guid adminId, CancellationToken ct = default)
    {
        var request = await withdrawalRepository.GetByIdAsync(requestId, ct)
            ?? throw new BusinessException("withdrawal.notFound");
        request.Approve(adminId);
        await withdrawalRepository.UpdateAsync(request, ct);
    }

    public async Task CompleteWithdrawalAsync(Guid requestId, Guid adminId, CancellationToken ct = default)
    {
        var request = await withdrawalRepository.GetByIdAsync(requestId, ct)
            ?? throw new BusinessException("withdrawal.notFound");
        var wallet = await walletRepository.GetByIdAsync(request.WalletId, ct)
            ?? throw new BusinessException("wallet.notFound");
        var tx = wallet.DeductFrozen(request.Amount, request.Id);
        request.Complete(adminId);
        await walletRepository.UpdateAsync(wallet, ct);
        await withdrawalRepository.UpdateAsync(request, ct);
        await txRepository.AddAsync(tx, ct);
    }

    public async Task RejectWithdrawalAsync(Guid requestId, Guid adminId, string reason, CancellationToken ct = default)
    {
        var request = await withdrawalRepository.GetByIdAsync(requestId, ct)
            ?? throw new BusinessException("withdrawal.notFound");
        var wallet = await walletRepository.GetByIdAsync(request.WalletId, ct)
            ?? throw new BusinessException("wallet.notFound");

        WalletTransaction? tx = null;
        // Pending 时余额已被 Freeze（RequestWithdrawal 时），Approved 时也处于冻结状态 → 需解冻
        if (request.Status == WithdrawalStatus.Pending || request.Status == WithdrawalStatus.Approved)
            tx = wallet.Unfreeze(request.Amount, request.Id);

        request.Reject(adminId, reason);
        await walletRepository.UpdateAsync(wallet, ct);
        await withdrawalRepository.UpdateAsync(request, ct);
        if (tx is not null)
            await txRepository.AddAsync(tx, ct);
    }

    public async Task<PagedResult<WalletTransactionDto>> GetTransactionsAsync(Guid investorId, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await txRepository.GetPagedByInvestorAsync(investorId, page, pageSize, ct);
        var dtos = items.Adapt<List<WalletTransactionDto>>();
        return new PagedResult<WalletTransactionDto> { Items = dtos, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResult<WithdrawalRequestDto>> GetWithdrawalRequestsAsync(string? status, int page, int pageSize, string? keyword = null, CancellationToken ct = default)
    {
        WithdrawalStatus? statusEnum = status is not null && Enum.TryParse<WithdrawalStatus>(status, out var s) ? s : null;
        IReadOnlyList<Guid>? investorIds = null;
        if (!string.IsNullOrWhiteSpace(keyword))
            investorIds = await userRepository.SearchIdsByKeywordAsync(keyword, ct);
        var (items, total) = await withdrawalRepository.GetPagedAsync(statusEnum, page, pageSize, investorIds, ct);
        var userIds = items.Select(w => w.InvestorUserId).Distinct().ToList();
        var users = await userRepository.GetByIdsAsync(userIds, ct);
        var userMap = users.ToDictionary(u => u.Id);

        var dtos = items.Select(w =>
        {
            var dto = w.Adapt<WithdrawalRequestDto>();
            if (userMap.TryGetValue(w.InvestorUserId, out var user))
                dto.InvestorName = user.DisplayName;
            return dto;
        }).ToList();

        return new PagedResult<WithdrawalRequestDto> { Items = dtos, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResult<WithdrawalRequestDto>> GetMyWithdrawalRequestsAsync(Guid investorId, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await withdrawalRepository.GetPagedByInvestorAsync(investorId, page, pageSize, ct);
        var dtos = items.Adapt<List<WithdrawalRequestDto>>();
        return new PagedResult<WithdrawalRequestDto> { Items = dtos, Total = total, Page = page, PageSize = pageSize };
    }

    // ── 私有方法 ──────────────────────────────────────────────

    private async Task<Domain.Entities.Wallet> GetOrCreateWalletAsync(Guid investorId, CancellationToken ct)
    {
        var wallet = await walletRepository.GetByInvestorIdAsync(investorId, ct);
        if (wallet is not null) return wallet;

        // 查询用户确认存在
        var user = await userRepository.GetByIdAsync(investorId, ct)
            ?? throw new BusinessException("user.notFound");

        // 默认货币：由 IRegionContext 提供，此处简化为 USD（可后续注入 IRegionContext）
        wallet = Domain.Entities.Wallet.Create(investorId, "USD");
        await walletRepository.AddAsync(wallet, ct);
        return wallet;
    }
}
