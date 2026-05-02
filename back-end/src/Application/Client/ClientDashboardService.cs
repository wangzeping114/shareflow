using ShareFlow.Application.Client.DTOs;
using ShareFlow.Application.Client.Interfaces;
using ShareFlow.Application.Common;
using ShareFlow.Application.Common.Interfaces;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Application.Client;

public class ClientDashboardService(
    IWalletRepository walletRepository,
    IDividendRepository dividendRepository,
    IContractRepository contractRepository,
    IVideoProjectRepository projectRepository,
    IStorageService storageService) : IClientDashboardService
{
    public async Task<ClientDashboardDto> GetDashboardAsync(Guid clientUserId, CancellationToken ct = default)
    {
        // 顺序拉取（EF Core DbContext 不支持并发操作）
        var wallet = await walletRepository.GetByInvestorIdAsync(clientUserId, ct);
        var contracts = await contractRepository.GetByClientIdAsync(clientUserId, ct);

        // 近期分红（最新 5 条）
        var (recentDividends, _) = await dividendRepository.GetPagedAsync(
            null, clientUserId, null, 1, 5, ct);

        // 汇总分红总额
        var totalDividend = recentDividends
            .Where(d => d.Status == DividendStatus.Distributed)
            .Sum(d => d.DividendAmount);

        // 构建项目摘要（按合同分组）
        var projectIds = contracts.Select(c => c.ProjectId).Distinct().ToList();
        var projectMap = new Dictionary<Guid, string>();
        var platformMap = new Dictionary<Guid, string>();

        foreach (var id in projectIds)
        {
            var proj = await projectRepository.GetByIdAsync(id, ct);
            if (proj is not null)
            {
                projectMap[id] = proj.Title;
                platformMap[id] = proj.PlatformName;
            }
        }

        // 按项目汇总分红
        var dividendByProject = new Dictionary<Guid, decimal>();
        {
            var (allDividends, _) = await dividendRepository.GetPagedAsync(
                projectIds, clientUserId, null, 1, 500, ct);
            foreach (var d in allDividends.Where(d => d.Status == DividendStatus.Distributed))
            {
                dividendByProject.TryGetValue(d.ProjectId, out var existing);
                dividendByProject[d.ProjectId] = existing + d.DividendAmount;
            }
        }

        var projectSummaries = contracts.Select(c => new ClientProjectSummaryDto
        {
            ProjectId = c.ProjectId,
            ProjectTitle = projectMap.GetValueOrDefault(c.ProjectId, "—"),
            PlatformName = platformMap.GetValueOrDefault(c.ProjectId, "—"),
            SharePermille = c.Slot?.SharePermille ?? 0,
            TotalDividendReceived = dividendByProject.GetValueOrDefault(c.ProjectId, 0),
            Currency = wallet?.Currency ?? "USD",
            ContractStatus = c.Status.ToString(),
        }).ToList();

        var recentDtos = recentDividends.Select(d => new ClientRecentDividendDto
        {
            Id = d.Id,
            ProjectTitle = projectMap.GetValueOrDefault(d.ProjectId, "—"),
            DividendAmount = d.DividendAmount,
            Currency = d.Currency,
            StatusLabel = GetDividendStatusLabel(d.Status),
            CalculatedAt = d.CalculatedAt,
        }).ToList();

        return new ClientDashboardDto
        {
            WalletBalance = wallet?.Balance ?? 0,
            FrozenAmount = wallet?.FrozenAmount ?? 0,
            Currency = wallet?.Currency ?? "USD",
            TotalDividendReceived = totalDividend,
            Projects = projectSummaries,
            RecentDividends = recentDtos,
        };
    }

    public async Task<PagedResult<ClientDividendDto>> GetDividendsAsync(
        Guid clientUserId, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await dividendRepository.GetPagedAsync(
            null, clientUserId, null, page, pageSize, ct);

        // 批量加载项目名称
        var projectIds = items.Select(d => d.ProjectId).Distinct().ToList();
        var projectMap = new Dictionary<Guid, (string Title, string Platform)>();
        foreach (var id in projectIds)
        {
            var proj = await projectRepository.GetByIdAsync(id, ct);
            if (proj is not null)
                projectMap[id] = (proj.Title, proj.PlatformName);
        }

        var dtos = items.Select(d =>
        {
            projectMap.TryGetValue(d.ProjectId, out var info);
            return new ClientDividendDto
            {
                Id = d.Id,
                ProjectId = d.ProjectId,
                ProjectTitle = info.Title ?? "—",
                PlatformName = info.Platform ?? "—",
                RevenueAmount = d.RevenueAmount,
                SharePermille = d.SharePermille,
                DividendAmount = d.DividendAmount,
                Currency = d.Currency,
                Status = d.Status.ToString(),
                StatusLabel = GetDividendStatusLabel(d.Status),
                CalculatedAt = d.CalculatedAt,
            };
        }).ToList();

        return new PagedResult<ClientDividendDto> { Items = dtos, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<IReadOnlyList<ClientContractDto>> GetContractsAsync(
        Guid clientUserId, CancellationToken ct = default)
    {
        var contracts = await contractRepository.GetByClientIdAsync(clientUserId, ct);

        var dtos = contracts.Select(c => new ClientContractDto
        {
            Id = c.Id,
            ProjectId = c.ProjectId,
            ProjectTitle = c.Project?.Title ?? "—",
            PlatformName = c.Project?.PlatformName ?? "—",
            SharePermille = c.Slot?.SharePermille ?? 0,
            Status = c.Status.ToString(),
            StatusLabel = GetContractStatusLabel(c.Status),
            SignedAt = c.SignedAt,
            HasPdf = !string.IsNullOrEmpty(c.PdfStoragePath),
        }).ToList();

        return dtos;
    }

    public async Task<string?> GetContractPdfPathAsync(Guid contractId, Guid clientUserId, CancellationToken ct = default)
    {
        var contract = await contractRepository.GetByIdAsync(contractId, ct);
        if (contract is null || contract.InvestorUserId != clientUserId)
            throw new BusinessException("contract.notFound");

        if (string.IsNullOrEmpty(contract.PdfStoragePath))
            return null;

        // 返回 MinIO 预签名 URL，有效期 1 小时
        return await storageService.GetPresignedUrlAsync(contract.PdfStoragePath, 3600, ct);
    }

    // ── Helpers ───────────────────────────────────────────────

    private static string GetDividendStatusLabel(DividendStatus s) => s switch
    {
        DividendStatus.Calculated => "待确认",
        DividendStatus.Confirmed  => "待发放",
        DividendStatus.Distributed => "已到账",
        _ => s.ToString(),
    };

    private static string GetContractStatusLabel(ContractStatus s) => s switch
    {
        ContractStatus.Draft        => "草稿",
        ContractStatus.Sent         => "待签署",
        ContractStatus.Signed       => "已签署",
        ContractStatus.Executed     => "生效中",
        ContractStatus.Expired      => "已到期",
        ContractStatus.PendingRenew => "等待续签",
        ContractStatus.Renewing     => "续签中",
        ContractStatus.Superseded   => "已取代",
        _ => s.ToString(),
    };
}
