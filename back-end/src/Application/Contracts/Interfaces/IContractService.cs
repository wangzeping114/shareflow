using ShareFlow.Application.Common;
using ShareFlow.Application.Contracts.DTOs;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Application.Contracts.Interfaces;

public interface IContractService
{
    Task<PagedResult<ContractDto>> GetListAsync(ContractQueryRequest query, CancellationToken ct = default);
    Task<ContractDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CreateAsync(CreateContractRequest request, Guid operatorId, CancellationToken ct = default);
    Task<GenerateSignLinkResult> GenerateSignLinkAsync(Guid contractId, CancellationToken ct = default);
    /// <summary>撤销合同，释放槽位</summary>
    Task CancelAsync(Guid contractId, CancellationToken ct = default);
    /// <summary>删除合同，释放槽位</summary>
    Task DeleteAsync(Guid contractId, CancellationToken ct = default);
    /// <summary>更换合同绑定的持股人</summary>
    Task ChangeInvestorAsync(Guid contractId, Guid newInvestorUserId, CancellationToken ct = default);
}
