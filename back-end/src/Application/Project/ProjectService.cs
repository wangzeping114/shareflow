using MapsterMapper;
using ShareFlow.Application.Common;
using ShareFlow.Application.Project.DTOs;
using ShareFlow.Application.Project.Interfaces;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Application.Project;

public class ProjectService(
    IVideoProjectRepository projectRepository,
    IProjectSlotRepository slotRepository,
    IMapper mapper) : IProjectService
{
    public async Task<PagedResult<ProjectDto>> GetListAsync(ProjectQueryRequest query, CancellationToken ct = default)
    {
        var paged = await projectRepository.GetPagedAsync(
            query.Title, query.Status, query.Page, query.PageSize, ct);

        return PagedResult<ProjectDto>.MapFrom(paged, query.Page, query.PageSize,
            items => mapper.Map<List<ProjectDto>>(items));
    }

    public async Task<ProjectDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var project = await projectRepository.GetByIdWithSlotsAsync(id, ct)
            ?? throw new BusinessException("项目不存在。", 404);

        return mapper.Map<ProjectDetailDto>(project);
    }

    public async Task<Guid> CreateAsync(CreateProjectRequest request, Guid operatorId, CancellationToken ct = default)
    {
        var project = VideoProject.Create(
            request.Title,
            request.Description,
            request.PlatformName,
            request.SlotMode,
            request.TotalSlots,
            operatorId,
            request.TotalInvestment);

        await projectRepository.AddAsync(project, ct);
        return project.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateProjectRequest request, Guid operatorId, CancellationToken ct = default)
    {
        var project = await projectRepository.GetByIdWithSlotsAsync(id, ct)
            ?? throw new BusinessException("项目不存在。", 404);

        project.Update(request.Title, request.Description, request.PlatformName, request.TotalSlots, request.TotalInvestment);
        await projectRepository.UpdateAsync(project, ct);
    }

    public async Task ChangeStatusAsync(Guid id, ChangeProjectStatusRequest request, Guid operatorId, CancellationToken ct = default)
    {
        var project = await projectRepository.GetByIdAsync(id, ct)
            ?? throw new BusinessException("项目不存在。", 404);

        switch (request.NewStatus)
        {
            case Domain.Enums.ProjectStatus.Active:
                project.Activate();
                break;
            case Domain.Enums.ProjectStatus.Paused:
                project.Pause();
                break;
            case Domain.Enums.ProjectStatus.Closed:
                project.Close();
                break;
            case Domain.Enums.ProjectStatus.Draft:
                project.Reopen();
                break;
            default:
                throw new BusinessException("不支持的状态变更。", 400);
        }

        await projectRepository.UpdateAsync(project, ct);
    }

    public async Task<SlotDto> AddSlotAsync(Guid projectId, AddSlotRequest request, CancellationToken ct = default)
    {
        var project = await projectRepository.GetByIdWithSlotsAsync(projectId, ct)
            ?? throw new BusinessException("项目不存在。", 404);

        var slot = project.AddSlot(request.SharePct);
        await slotRepository.AddAsync(slot, ct);
        await projectRepository.UpdateAsync(project, ct);

        return mapper.Map<SlotDto>(slot);
    }

    public async Task<SlotDto> UpdateSlotContractMonthsAsync(Guid projectId, Guid slotId, UpdateSlotContractMonthsRequest request, CancellationToken ct = default)
    {
        var slot = await slotRepository.GetByIdAsync(slotId, ct)
            ?? throw new BusinessException("槽位不存在。", 404);

        if (slot.ProjectId != projectId)
            throw new BusinessException("槽位不属于该项目。", 400);

        slot.SetContractMonths(request.ContractMonths);
        await slotRepository.UpdateAsync(slot, ct);

        return mapper.Map<SlotDto>(slot);
    }

    public async Task<SlotDto> UpdateSlotAliasAsync(Guid projectId, Guid slotId, UpdateSlotAliasRequest request, CancellationToken ct = default)
    {
        var slot = await slotRepository.GetByIdAsync(slotId, ct)
            ?? throw new BusinessException("槽位不存在。", 404);

        if (slot.ProjectId != projectId)
            throw new BusinessException("槽位不属于该项目。", 400);

        slot.SetAlias(request.Alias);
        await slotRepository.UpdateAsync(slot, ct);

        return mapper.Map<SlotDto>(slot);
    }

    public async Task<IReadOnlyList<SlotDto>> BatchUpdateSlotsAsync(Guid projectId, BatchUpdateSlotsRequest request, CancellationToken ct = default)
    {
        if (request.SlotIds is not { Count: > 0 })
            throw new BusinessException("请至少选择一个槽位。", 400);

        var slots = await slotRepository.GetByIdsAsync(request.SlotIds, ct);
        var projectSlots = slots.Where(s => s.ProjectId == projectId).ToList();

        foreach (var slot in projectSlots)
        {
            if (request.ContractMonths.HasValue)
                slot.SetContractMonths(request.ContractMonths.Value);
            if (request.TemplateType is not null)
                slot.SetTemplateType(request.TemplateType);
            if (request.SharePct.HasValue)
                slot.SetSharePermille(request.SharePct.Value);
        }

        await slotRepository.UpdateRangeAsync(projectSlots, ct);
        return projectSlots.Select(mapper.Map<SlotDto>).ToList();
    }}