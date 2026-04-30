using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Interfaces;

public interface IContractRepository
{
    Task<Contract?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Contract?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<Contract?> GetBySignTokenAsync(string token, CancellationToken ct = default);
    Task<(IReadOnlyList<Contract> Items, int Total)> GetPagedAsync(
        Guid? projectId, ContractStatus? status, string? projectTitle, int page, int pageSize, CancellationToken ct = default);
    /// <summary>返回所有已签署或已执行的合同（用于槽位补修复）</summary>
    Task<IReadOnlyList<Contract>> GetSignedAsync(CancellationToken ct = default);
    Task AddAsync(Contract contract, CancellationToken ct = default);
    Task UpdateAsync(Contract contract, CancellationToken ct = default);
}
