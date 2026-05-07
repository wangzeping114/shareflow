using ShareFlow.Application.Common;
using ShareFlow.Application.Sales.DTOs;

namespace ShareFlow.Application.Sales.Interfaces;

public interface ISalesService
{
    Task<PagedResult<LeadDto>> GetLeadsAsync(Guid salesUserId, LeadQueryRequest query, CancellationToken ct = default);
    Task<LeadDto> CreateLeadAsync(Guid salesUserId, CreateLeadRequest request, CancellationToken ct = default);
    Task<LeadDto> UpdateLeadAsync(Guid salesUserId, Guid leadId, UpdateLeadRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<SalesProjectDto>> GetAvailableProjectsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SalesClientDto>> GetClientsAsync(Guid salesUserId, CancellationToken ct = default);
    Task<IReadOnlyList<SalesContractDto>> GetContractsAsync(Guid salesUserId, CancellationToken ct = default);
    Task<SalesContractDetailDto> GetContractDetailAsync(Guid salesUserId, Guid contractId, CancellationToken ct = default);
    Task<InitiateContractResult> InitiateContractAsync(Guid salesUserId, InitiateContractRequest request, CancellationToken ct = default);
    Task<SalesPerformanceDto> GetPerformanceAsync(Guid salesUserId, CancellationToken ct = default);
    /// <summary>撤销合同，释放槽位</summary>
    Task CancelContractAsync(Guid salesUserId, Guid contractId, CancellationToken ct = default);
    /// <summary>删除合同，释放槽位</summary>
    Task DeleteContractAsync(Guid salesUserId, Guid contractId, CancellationToken ct = default);
    /// <summary>更换合同绑定的客户</summary>
    Task ChangeContractClientAsync(Guid salesUserId, Guid contractId, Guid newClientId, CancellationToken ct = default);
}
