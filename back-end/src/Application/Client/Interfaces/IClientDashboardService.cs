using ShareFlow.Application.Client.DTOs;
using ShareFlow.Application.Common;

namespace ShareFlow.Application.Client.Interfaces;

public interface IClientDashboardService
{
    Task<ClientDashboardDto> GetDashboardAsync(Guid clientUserId, CancellationToken ct = default);
    Task<PagedResult<ClientDividendDto>> GetDividendsAsync(Guid clientUserId, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<ClientContractDto>> GetContractsAsync(Guid clientUserId, CancellationToken ct = default);
    /// <summary>返回合同 PDF 存储路径（用于文件下载）</summary>
    Task<string?> GetContractPdfPathAsync(Guid contractId, Guid clientUserId, CancellationToken ct = default);

    /// <summary>返回合同 PDF 文件内容（用于在线预览）</summary>
    Task<byte[]?> GetContractPdfContentAsync(Guid contractId, Guid clientUserId, CancellationToken ct = default);
}
