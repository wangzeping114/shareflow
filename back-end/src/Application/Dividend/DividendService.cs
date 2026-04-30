using MapsterMapper;
using ShareFlow.Application.Common;
using ShareFlow.Application.Dividend.DTOs;
using ShareFlow.Application.Dividend.Interfaces;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Application.Dividend;

public class DividendService(
    IDividendRepository dividendRepository,
    IMapper mapper) : IDividendService
{
    public async Task<PagedResult<DividendDto>> GetListAsync(DividendQueryRequest query, CancellationToken ct = default)
    {
        var paged = await dividendRepository.GetPagedAsync(
            query.ProjectIds, query.InvestorUserId, query.Statuses, query.Page, query.PageSize, ct);

        return PagedResult<DividendDto>.MapFrom(paged, query.Page, query.PageSize,
            items => mapper.Map<List<DividendDto>>(items));
    }

    public async Task<DividendProjectSummaryDto> GetProjectSummaryAsync(Guid projectId, CancellationToken ct = default)
    {
        var summary = await dividendRepository.GetProjectSummaryAsync(projectId, ct);
        return mapper.Map<DividendProjectSummaryDto>(summary);
    }
}
