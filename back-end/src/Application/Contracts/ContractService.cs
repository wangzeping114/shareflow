using MapsterMapper;
using ShareFlow.Application.Common;
using ShareFlow.Application.Contracts.DTOs;
using ShareFlow.Application.Contracts.Interfaces;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;

// ReSharper disable once RedundantUsingDirective (RegionMode enum lives here)

namespace ShareFlow.Application.Contracts;

public class ContractService(
    IContractRepository contractRepository,
    IVideoProjectRepository projectRepository,
    IUserRepository userRepository,
    IProjectSlotRepository slotRepository,
    IRegionContext regionContext,
    IContractTemplateService templateService,
    IMapper mapper) : IContractService
{
    public async Task<PagedResult<ContractDto>> GetListAsync(ContractQueryRequest query, CancellationToken ct = default)
    {
        var paged = await contractRepository.GetPagedAsync(
            query.ProjectId, query.Status, query.ProjectTitle, query.Page, query.PageSize, ct);

        return PagedResult<ContractDto>.MapFrom(paged, query.Page, query.PageSize,
            items => mapper.Map<List<ContractDto>>(items));
    }

    public async Task<ContractDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var contract = await contractRepository.GetByIdWithDetailsAsync(id, ct)
            ?? throw new BusinessException("合同不存在。", 404);

        return mapper.Map<ContractDetailDto>(contract);
    }

    public async Task<Guid> CreateAsync(CreateContractRequest request, Guid operatorId, CancellationToken ct = default)
    {
        // 校验项目和槽位存在
        var project = await projectRepository.GetByIdWithSlotsAsync(request.ProjectId, ct)
            ?? throw new BusinessException("项目不存在。", 404);

        var slot = project.Slots.FirstOrDefault(s => s.Id == request.SlotId)
            ?? throw new BusinessException("槽位不存在。", 404);

        var investor = await userRepository.GetByIdAsync(request.InvestorUserId, ct)
            ?? throw new BusinessException("投资人用户不存在。", 404);

        // 根据 region 决定模板类型（overseas 默认英文）
        var templateType = regionContext.Mode == RegionMode.Overseas
            ? "OverseasEnglish"
            : "DomesticChinese";

        // 渲染标准化合同正文作为快照
        var templateData = new ContractTemplateData(
            ProjectTitle: project.Title,
            PlatformName: project.PlatformName,
            InvestorName: investor.DisplayName,
            SharePermille: slot.SharePermille,
            Currency: regionContext.DefaultCurrency,
            TotalInvestment: project.TotalInvestment ?? 0m,
            EffectiveDate: DateTime.UtcNow.ToString("yyyy-MM-dd"));

        var snapshot = templateService.Render(templateType, templateData);

        var contract = Contract.Create(
            request.ProjectId,
            request.SlotId,
            request.InvestorUserId,
            templateType,
            snapshot);

        await contractRepository.AddAsync(contract, ct);

        // 槽位预留给投资人，防止重复签约
        if (slot.Status == SlotStatus.Available)
        {
            slot.Reserve(request.InvestorUserId);
            await slotRepository.UpdateAsync(slot, ct);
        }

        return contract.Id;
    }

    public async Task<GenerateSignLinkResult> GenerateSignLinkAsync(Guid contractId, CancellationToken ct = default)
    {
        var contract = await contractRepository.GetByIdAsync(contractId, ct)
            ?? throw new BusinessException("合同不存在。", 404);

        var token = contract.GenerateSignToken();
        await contractRepository.UpdateAsync(contract, ct);

        // 签约 URL 前端路由：/esign/{token}
        var signUrl = $"/esign/{token}";

        return new GenerateSignLinkResult
        {
            SignUrl = signUrl,
            ExpiresAt = contract.SignTokenExpiresAt!.Value,
        };
    }
}
