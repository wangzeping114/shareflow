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
}
