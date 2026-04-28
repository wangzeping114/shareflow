using Mapster;
using ShareFlow.Application.Revenue.DTOs;
using ShareFlow.Domain.Entities;

namespace ShareFlow.Application.Revenue;

public class RevenueMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PlatformRevenue, RevenueDto>()
            .Map(dst => dst.ProjectTitle, src => src.Project != null ? src.Project.Title : string.Empty)
            .Map(dst => dst.ImportSource, src => src.ImportSource.ToString())
            .Map(dst => dst.Status, src => src.Status.ToString());
    }
}
