using MediatR;
using ShareFlow.Application.Contracts.DTOs;
using ShareFlow.Application.Contracts.Interfaces;
using ShareFlow.Domain.Events;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Infrastructure.ESign;

public class ESignService(IContractRepository contractRepo, IMediator mediator) : IESignService
{
    public async Task<ContractPreviewDto> GetPreviewAsync(string token, CancellationToken ct = default)
    {
        var contract = await contractRepo.GetBySignTokenAsync(token, ct)
            ?? throw new BusinessException("签约链接无效或已过期", 404);

        if (contract.SignTokenExpiresAt.HasValue && contract.SignTokenExpiresAt.Value < DateTime.UtcNow)
            throw new BusinessException("签约链接已过期", 400);

        return new ContractPreviewDto
        {
            ContractId = contract.Id,
            ProjectTitle = contract.Project?.Title ?? string.Empty,
            InvestorName = contract.InvestorUser?.DisplayName ?? string.Empty,
            SharePct = contract.Slot?.SharePermille ?? 0,
            TemplateType = contract.TemplateType,
            ContractSnapshot = contract.ContractSnapshot,
            ExpiresAt = contract.SignTokenExpiresAt ?? DateTime.UtcNow
        };
    }

    public async Task SignAsync(string token, string signatureDataUrl, CancellationToken ct = default)
    {
        var contract = await contractRepo.GetBySignTokenAsync(token, ct)
            ?? throw new BusinessException("签约链接无效或已过期", 404);

        contract.Sign(signatureDataUrl);

        await contractRepo.UpdateAsync(contract, ct);

        await mediator.Publish(
            new ContractSignedEvent(contract.Id, contract.InvestorUserId, contract.ProjectId, contract.SlotId), ct);
    }
}
