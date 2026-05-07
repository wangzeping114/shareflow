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
    /// <summary>返回指定客户的合同列表</summary>
    Task<IReadOnlyList<Contract>> GetByClientIdAsync(Guid clientUserId, CancellationToken ct = default);
    /// <summary>按项目+槽位+投资人查询未签署完成（Draft/Sent）合同，用于避免重复创建</summary>
    Task<Contract?> GetActiveUnsignedAsync(Guid projectId, Guid slotId, Guid investorUserId, CancellationToken ct = default);
    Task AddAsync(Contract contract, CancellationToken ct = default);
    Task UpdateAsync(Contract contract, CancellationToken ct = default);
    Task DeleteAsync(Contract contract, CancellationToken ct = default);
}
