using Mapster;
using ShareFlow.Application.Dividend.DTOs;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Application.Dividend;

public class DividendMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DividendRecord, DividendDto>()
            .Map(dst => dst.ProjectTitle, src => src.Project != null ? src.Project.Title : string.Empty)
            .Map(dst => dst.InvestorName,
                src => src.InvestorUser != null ? src.InvestorUser.DisplayName : string.Empty)
            .Map(dst => dst.Status, src => src.Status.ToString());

        config.NewConfig<DividendProjectSummary, DividendProjectSummaryDto>();
    }
}
