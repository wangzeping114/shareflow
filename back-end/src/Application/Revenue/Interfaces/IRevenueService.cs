using ShareFlow.Application.Revenue.DTOs;
using ShareFlow.Application.Common;

namespace ShareFlow.Application.Revenue.Interfaces;

public interface IRevenueService
{
    Task<Guid> AddManualAsync(AddManualRevenueRequest req, Guid operatorId, CancellationToken ct = default);
    Task<BatchImportResult> ImportCsvAsync(Guid projectId, string platform, Stream csvStream, CancellationToken ct = default);
    Task<AiImportPreviewDto> PreviewScreenshotAsync(Guid projectId, string platform, string imageBase64, string mimeType, CancellationToken ct = default);
    Task ConfirmAiImportAsync(Guid revenueId, decimal? overrideAmount, CancellationToken ct = default);
    Task ApproveAsync(Guid revenueId, Guid verifierId, CancellationToken ct = default);
    Task RejectAsync(Guid revenueId, Guid verifierId, string reason, CancellationToken ct = default);
    Task<PagedResult<RevenueDto>> GetListAsync(RevenueQueryRequest query, CancellationToken ct = default);
}
