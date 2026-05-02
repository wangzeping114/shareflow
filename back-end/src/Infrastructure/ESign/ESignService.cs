using MediatR;
using ShareFlow.Application.Contracts.DTOs;
using ShareFlow.Application.Contracts.Interfaces;
using ShareFlow.Domain.Events;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Interfaces;
using ShareFlow.Infrastructure.Contracts;

namespace ShareFlow.Infrastructure.ESign;

public class ESignService(
    IContractRepository contractRepo,
    IUserRepository userRepo,
    IMediator mediator) : IESignService
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

    public async Task<SignContractResult> SignAsync(string token, string signatureDataUrl, CancellationToken ct = default)
    {
        var contract = await contractRepo.GetBySignTokenAsync(token, ct)
            ?? throw new BusinessException("签约链接无效或已过期", 404);

        contract.Sign(signatureDataUrl);

        // 将签名日期嵌入快照
        if (!string.IsNullOrEmpty(contract.ContractSnapshot) && contract.SignedAt.HasValue)
        {
            var signedDateStr = contract.SignedAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            var updatedSnapshot = ContractTemplateService.EmbedSignature(contract.ContractSnapshot, signedDateStr);
            contract.UpdateSnapshot(updatedSnapshot);
        }

        await contractRepo.UpdateAsync(contract, ct);

        // 获取初始账号信息（仅首次签约时有值，取后清空 DB）
        string? clientUsername = null;
        string? initialPassword = null;
        var investorUser = await userRepo.GetByIdAsync(contract.InvestorUserId, ct);
        if (investorUser is not null && investorUser.InitialPassword is not null)
        {
            clientUsername = investorUser.Username;
            initialPassword = investorUser.TakeInitialPassword();
            await userRepo.UpdateAsync(investorUser, ct);
        }

        await mediator.Publish(
            new ContractSignedEvent(contract.Id, contract.InvestorUserId, contract.ProjectId, contract.SlotId), ct);

        return new SignContractResult
        {
            ClientUsername = clientUsername,
            InitialPassword = initialPassword
        };
    }
}
