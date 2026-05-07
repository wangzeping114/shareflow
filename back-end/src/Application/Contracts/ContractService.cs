using MapsterMapper;
using Microsoft.Extensions.Configuration;
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
    IMapper mapper,
    IConfiguration configuration) : IContractService
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

        // 幂等检查：若已存在未签约合同，直接返回其 ID，避免重复创建
        var existing = await contractRepository.GetActiveUnsignedAsync(
            request.ProjectId, request.SlotId, request.InvestorUserId, ct);
        if (existing is not null)
            return existing.Id;

        // 使用槽位自身配置的模板类型（由管理员在创建槽位时设定）
        var templateType = slot.TemplateType;

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
        var baseUrl = configuration["FrontendBaseUrl"]?.TrimEnd('/') ?? "";
        var signUrl = $"{baseUrl}/esign/{token}";

        return new GenerateSignLinkResult
        {
            SignUrl = signUrl,
            ExpiresAt = contract.SignTokenExpiresAt!.Value,
        };
    }

    public async Task CancelAsync(Guid contractId, CancellationToken ct = default)
    {
        var contract = await contractRepository.GetByIdAsync(contractId, ct)
            ?? throw new BusinessException("合同不存在。", 404);
        contract.Cancel();
        await contractRepository.UpdateAsync(contract, ct);
        await ReleaseSlotAsync(contract.SlotId, ct);
    }

    public async Task DeleteAsync(Guid contractId, CancellationToken ct = default)
    {
        var contract = await contractRepository.GetByIdAsync(contractId, ct)
            ?? throw new BusinessException("合同不存在。", 404);
        if (contract.Status == ContractStatus.Signed || contract.Status == ContractStatus.Executed)
            throw new BusinessException("已签署/执行的合同无法删除。", 400);
        await contractRepository.DeleteAsync(contract, ct);
        await ReleaseSlotAsync(contract.SlotId, ct);
    }

    public async Task ChangeInvestorAsync(Guid contractId, Guid newInvestorUserId, CancellationToken ct = default)
    {
        var contract = await contractRepository.GetByIdAsync(contractId, ct)
            ?? throw new BusinessException("合同不存在。", 404);
        var investor = await userRepository.GetByIdAsync(newInvestorUserId, ct)
            ?? throw new BusinessException("用户不存在。", 404);
        // 如果客户变更，需要释放旧槽位绑定并重新预留
        if (contract.InvestorUserId != newInvestorUserId)
        {
            var slot = await slotRepository.GetByIdAsync(contract.SlotId, ct);
            if (slot is not null && (slot.Status == SlotStatus.Reserved || slot.Status == SlotStatus.Occupied))
            {
                slot.Release();
                slot.Reserve(newInvestorUserId);
                await slotRepository.UpdateAsync(slot, ct);
            }
        }
        contract.ChangeInvestor(newInvestorUserId);
        await contractRepository.UpdateAsync(contract, ct);
    }

    private async Task ReleaseSlotAsync(Guid slotId, CancellationToken ct)
    {
        var slot = await slotRepository.GetByIdAsync(slotId, ct);
        if (slot is not null && slot.Status != SlotStatus.Available)
        {
            slot.Release();
            await slotRepository.UpdateAsync(slot, ct);
        }
    }
}
