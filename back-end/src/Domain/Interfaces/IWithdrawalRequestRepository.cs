using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Interfaces;

public interface IWithdrawalRequestRepository
{
    Task<WithdrawalRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<WithdrawalRequest> Items, int Total)> GetPagedAsync(
        WithdrawalStatus? status, int page, int pageSize, IReadOnlyList<Guid>? investorIds = null, CancellationToken ct = default);
    Task<(IReadOnlyList<WithdrawalRequest> Items, int Total)> GetPagedByInvestorAsync(
        Guid investorUserId, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(WithdrawalRequest request, CancellationToken ct = default);
    Task UpdateAsync(WithdrawalRequest request, CancellationToken ct = default);
}
