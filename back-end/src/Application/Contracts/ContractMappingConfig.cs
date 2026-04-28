using Mapster;
using ShareFlow.Application.Contracts.DTOs;
using ShareFlow.Domain.Entities;

namespace ShareFlow.Application.Contracts;

public class ContractMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Contract, ContractDto>()
            .Map(dst => dst.ProjectTitle, src => src.Project != null ? src.Project.Title : string.Empty)
            .Map(dst => dst.InvestorName, src => src.InvestorUser != null ? src.InvestorUser.DisplayName : string.Empty)
            .Map(dst => dst.HasPdf, src => src.PdfStoragePath != null);

        config.NewConfig<Contract, ContractDetailDto>()
            .Map(dst => dst.ProjectTitle, src => src.Project != null ? src.Project.Title : string.Empty)
            .Map(dst => dst.InvestorName, src => src.InvestorUser != null ? src.InvestorUser.DisplayName : string.Empty)
            .Map(dst => dst.HasPdf, src => src.PdfStoragePath != null)
            .Map(dst => dst.SignUrl, src =>
                src.SignToken != null ? $"/esign/{src.SignToken}" : null);
    }
}
