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
}
