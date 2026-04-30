using ShareFlow.Application.Common;
using ShareFlow.Application.Dividend.DTOs;
using ShareFlow.Domain.Common;

namespace ShareFlow.Application.Dividend.Interfaces;

public interface IDividendService
{
    Task<PagedResult<DividendDto>> GetListAsync(DividendQueryRequest query, CancellationToken ct = default);
    Task<DividendProjectSummaryDto> GetProjectSummaryAsync(Guid projectId, CancellationToken ct = default);
}
