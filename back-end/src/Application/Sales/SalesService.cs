using Microsoft.Extensions.Configuration;
using ShareFlow.Application.Common;
using ShareFlow.Application.Contracts.DTOs;
using ShareFlow.Application.Contracts.Interfaces;
using ShareFlow.Application.Sales.DTOs;
using ShareFlow.Application.Sales.Interfaces;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Application.Sales;

public class SalesService(
    ILeadRepository leadRepository,
    IVideoProjectRepository projectRepository,
    IProjectSlotRepository slotRepository,
    IUserRepository userRepository,
    IContractRepository contractRepository,
    IContractService contractService,
    IConfiguration configuration) : ISalesService
{
    public async Task<PagedResult<LeadDto>> GetLeadsAsync(Guid salesUserId, LeadQueryRequest query, CancellationToken ct = default)
    {
        var (items, total) = await leadRepository.GetPagedAsync(salesUserId, query.Status, query.Page, query.PageSize, ct);
        return new PagedResult<LeadDto>
        {
            Items = items.Select(MapToDto).ToList(),
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<LeadDto> CreateLeadAsync(Guid salesUserId, CreateLeadRequest request, CancellationToken ct = default)
    {
        var lead = Lead.Create(request.Name, request.ContactInfo, request.Email, salesUserId, request.Notes);
        await leadRepository.AddAsync(lead, ct);
        return MapToDto(lead);
    }

    public async Task<LeadDto> UpdateLeadAsync(Guid salesUserId, Guid leadId, UpdateLeadRequest request, CancellationToken ct = default)
    {
        var lead = await leadRepository.GetByIdAsync(leadId, ct)
            ?? throw new BusinessException("lead.notFound");

        if (lead.SalesOwnerId != salesUserId)
            throw new BusinessException("lead.accessDenied");

        lead.Update(request.Name, request.ContactInfo, request.Email, request.Notes);
        lead.ChangeStatus(request.Status);
        await leadRepository.UpdateAsync(lead, ct);
        return MapToDto(lead);
    }

    public async Task<IReadOnlyList<SalesProjectDto>> GetAvailableProjectsAsync(CancellationToken ct = default)
    {
        var paged = await projectRepository.GetPagedAsync(null, ProjectStatus.Active, 1, 100, ct);

        var result = new List<SalesProjectDto>();
        foreach (var p in paged.Items)
        {
            var slots = await slotRepository.GetByProjectIdAsync(p.Id, ct);
            var availableSlots = slots
                .Where(s => s.Status == SlotStatus.Available)
                .Select(s => new SalesProjectSlotDto
                {
                    Id = s.Id,
                    SlotNumber = s.SlotNumber,
                    Alias = s.Alias,
                    SharePermille = s.SharePermille
                })
                .ToList();

            result.Add(new SalesProjectDto
            {
                Id = p.Id,
                Title = p.Title,
                PlatformName = p.PlatformName,
                AvailableSlots = availableSlots.Count,
                Slots = availableSlots,
                CreatedAt = p.CreatedAt
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<SalesClientDto>> GetClientsAsync(Guid salesUserId, CancellationToken ct = default)
    {
        var (leads, _) = await leadRepository.GetPagedAsync(salesUserId, null, 1, 1000, ct);
        return leads
            .OrderBy(x => x.Name)
            .Select(x => new SalesClientDto
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email ?? string.Empty,
                HasAccount = x.ClientUserId.HasValue
            })
            .ToList();
    }

    public async Task<IReadOnlyList<SalesContractDto>> GetContractsAsync(Guid salesUserId, CancellationToken ct = default)
    {
        var (leads, _) = await leadRepository.GetPagedAsync(salesUserId, null, 1, 1000, ct);

        var clientNameMap = leads
            .Where(x => x.ClientUserId.HasValue)
            .GroupBy(x => x.ClientUserId!.Value)
            .ToDictionary(x => x.Key, x => x.First().Name);

        var contracts = new List<Contract>();
        foreach (var clientUserId in clientNameMap.Keys)
        {
            var list = await contractRepository.GetByClientIdAsync(clientUserId, ct);
            contracts.AddRange(list);
        }

        return contracts
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SalesContractDto
            {
                Id = x.Id,
                ContractNo = x.ContractNo,
                ProjectTitle = x.Project?.Title ?? string.Empty,
                SlotId = x.SlotId,
                SlotNumber = x.Slot?.SlotNumber ?? 0,
                SlotAlias = x.Slot?.Alias,
                SharePermille = x.Slot?.SharePermille ?? 0m,
                ClientName = clientNameMap.TryGetValue(x.InvestorUserId, out var name) ? name : string.Empty,
                Status = x.Status,
                SignedAt = x.SignedAt,
                ExpiresAt = x.SignTokenExpiresAt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToList();
    }

    public async Task<SalesContractDetailDto> GetContractDetailAsync(Guid salesUserId, Guid contractId, CancellationToken ct = default)
    {
        var contract = await contractRepository.GetByIdWithDetailsAsync(contractId, ct)
            ?? throw new BusinessException("contract.notFound", 404);

        // 验证权限：合同的投资人必须是该销售所属的 Lead 的客户用户
        var (leads, _) = await leadRepository.GetPagedAsync(salesUserId, null, 1, 1000, ct);
        var clientUserIds = leads.Where(l => l.ClientUserId.HasValue).Select(l => l.ClientUserId!.Value).ToHashSet();
        if (!clientUserIds.Contains(contract.InvestorUserId))
            throw new BusinessException("contract.accessDenied", 403);

        var clientLead = leads.FirstOrDefault(l => l.ClientUserId == contract.InvestorUserId);

        // 获取客户用户名
        string? clientUsername = null;
        if (contract.InvestorUser is not null)
            clientUsername = contract.InvestorUser.Username;
        else if (clientLead?.ClientUserId.HasValue == true)
        {
            var user = await userRepository.GetByIdAsync(clientLead.ClientUserId!.Value, ct);
            clientUsername = user?.Username;
        }

        return new SalesContractDetailDto
        {
            Id = contract.Id,
            ContractNo = contract.ContractNo,
            ProjectTitle = contract.Project?.Title ?? string.Empty,
            ClientName = clientLead?.Name ?? string.Empty,
            Status = contract.Status.ToString(),
            SharePermille = contract.Slot?.SharePermille ?? 0m,
            ContractSnapshot = contract.ContractSnapshot,
            SignatureDataUrl = contract.SignatureDataUrl,
            SignedAt = contract.SignedAt,
            CreatedAt = contract.CreatedAt,
            ClientUsername = clientUsername,
            ClientInitialPassword = clientLead?.ClientInitialPassword,
            SignUrl = contract.SignToken is not null && contract.SignTokenExpiresAt > DateTime.UtcNow
                ? $"{(configuration["FrontendBaseUrl"]?.TrimEnd('/') ?? "")}/esign/{contract.SignToken}"
                : null,
            SignTokenExpiresAt = contract.SignTokenExpiresAt
        };
    }

    public async Task<InitiateContractResult> InitiateContractAsync(Guid salesUserId, InitiateContractRequest request, CancellationToken ct = default)
    {
        // 获取 Lead（InvestorUserId 实际上是 LeadId）
        var lead = await leadRepository.GetByIdAsync(request.InvestorUserId, ct)
            ?? throw new BusinessException("lead.notFound");

        // 检查销售员权限
        if (lead.SalesOwnerId != salesUserId)
            throw new BusinessException("lead.accessDenied");

        // 如果 Lead 还没有关联的 Client 用户，创建一个
        string? newTempPassword = null;
        string? newClientUsername = null;
        Guid investorUserId;
        if (lead.ClientUserId.HasValue)
        {
            investorUserId = lead.ClientUserId.Value;
        }
        else
        {
            var (userId, tempPwd) = await CreateClientForLeadAsync(lead, ct);
            investorUserId = userId;
            newTempPassword = tempPwd;
            // 获取新创建的用户名
            var createdUser = await userRepository.GetByIdAsync(userId, ct);
            newClientUsername = createdUser?.Username;
        }

        var contractId = await contractService.CreateAsync(
            new CreateContractRequest
            {
                ProjectId = request.ProjectId,
                SlotId = request.SlotId,
                InvestorUserId = investorUserId
            },
            salesUserId,
            ct);

        var signResult = await contractService.GenerateSignLinkAsync(contractId, ct);

        return new InitiateContractResult
        {
            ContractId = contractId,
            ContractNo = signResult.ContractNo,
            SignUrl = signResult.SignUrl,
            ExpiresAt = signResult.ExpiresAt,
            ClientUsername = newClientUsername,
            TempPassword = newTempPassword
        };
    }

    private async Task<(Guid UserId, string TempPassword)> CreateClientForLeadAsync(Lead lead, CancellationToken ct)
    {
        // 使用 Lead 的邮箱或名称生成用户名
        var username = lead.Email?.Split('@')[0] ?? lead.Name.ToLower().Replace(" ", "");

        var clientUser = User.CreateClient(username, lead.Email ?? $"{username}@shareflow.local", lead.Name, out var tempPassword);

        await userRepository.AddAsync(clientUser, ct);
        
        // 更新 Lead 的 ClientUserId 和初始密码（销售侧永久保存）
        lead.SetClientUserId(clientUser.Id);
        lead.SetClientInitialPassword(tempPassword);
        await leadRepository.UpdateAsync(lead, ct);

        return (clientUser.Id, tempPassword);
    }

    public async Task<SalesPerformanceDto> GetPerformanceAsync(Guid salesUserId, CancellationToken ct = default)
    {
        var total = await leadRepository.CountByStatusAsync(salesUserId, null);
        var converted = await leadRepository.CountByStatusAsync(salesUserId, LeadStatus.Converted);
        var rate = total == 0 ? 0m : Math.Round((decimal)converted / total * 100, 2);

        // 合同统计：通过所有合同中 salesUserId 创建的（需要 ContractService 支持，此处从合同列表中过滤）
        // 简化实现：使用 ContractService 查询页数据，按 operatorId 过滤暂未支持，返回 0
        // TODO: 扩展 ContractService/Repository 支持按 createdByUserId 过滤
        return new SalesPerformanceDto
        {
            TotalLeads = total,
            ConvertedLeads = converted,
            ConversionRate = rate,
            ContractsSent = 0,
            ContractsSigned = 0
        };
    }

    private static LeadDto MapToDto(Lead lead) => new()
    {
        Id = lead.Id,
        Name = lead.Name,
        ContactInfo = lead.ContactInfo,
        Email = lead.Email,
        Status = lead.Status,
        Notes = lead.Notes,
        CreatedAt = lead.CreatedAt,
        UpdatedAt = lead.UpdatedAt
    };

    private async Task VerifyContractOwnershipAsync(Guid salesUserId, Guid contractId, CancellationToken ct)
    {
        var contract = await contractRepository.GetByIdAsync(contractId, ct)
            ?? throw new BusinessException("contract.notFound", 404);
        var (leads, _) = await leadRepository.GetPagedAsync(salesUserId, null, 1, 1000, ct);
        var clientUserIds = leads.Where(l => l.ClientUserId.HasValue).Select(l => l.ClientUserId!.Value).ToHashSet();
        if (!clientUserIds.Contains(contract.InvestorUserId))
            throw new BusinessException("contract.accessDenied", 403);
    }

    public async Task CancelContractAsync(Guid salesUserId, Guid contractId, CancellationToken ct = default)
    {
        await VerifyContractOwnershipAsync(salesUserId, contractId, ct);
        await contractService.CancelAsync(contractId, ct);
    }

    public async Task DeleteContractAsync(Guid salesUserId, Guid contractId, CancellationToken ct = default)
    {
        await VerifyContractOwnershipAsync(salesUserId, contractId, ct);
        await contractService.DeleteAsync(contractId, ct);
    }

    public async Task ChangeContractClientAsync(Guid salesUserId, Guid contractId, Guid newLeadId, CancellationToken ct = default)
    {
        await VerifyContractOwnershipAsync(salesUserId, contractId, ct);
        // 新客户必须属于该销售的线索，且已关联 ClientUser
        var (leads, _) = await leadRepository.GetPagedAsync(salesUserId, null, 1, 1000, ct);
        var newLead = leads.FirstOrDefault(l => l.Id == newLeadId)
            ?? throw new BusinessException("客户不属于该销售的线索列表。", 400);
        if (!newLead.ClientUserId.HasValue)
            throw new BusinessException("该客户尚未创建登录账号，请先完成合同签约。", 400);
        await contractService.ChangeInvestorAsync(contractId, newLead.ClientUserId.Value, ct);
    }
}
