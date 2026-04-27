using Mapster;
using ShareFlow.Application.Project.DTOs;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Application.Project;

public class ProjectMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<VideoProject, ProjectDto>()
            .Map(dest => dest.SlotMode, src => src.SlotMode.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.FundingProgressPct,
                src => src.TotalSlots == 0 ? 0m :
                    Math.Round((decimal)src.FilledSlots / src.TotalSlots * 100, 1));

        config.NewConfig<VideoProject, ProjectDetailDto>()
            .Inherits<VideoProject, ProjectDto>()
            .Map(dest => dest.Slots, src => src.Slots);

        config.NewConfig<ProjectSlot, SlotDto>()
            .Map(dest => dest.SharePct, src => src.SharePermille)
            .Map(dest => dest.Status, src => src.Status.ToString());
    }
}
