using MapsterMapper;
using MediatR;
using ShareFlow.Application.Common;
using ShareFlow.Application.Revenue.DTOs;
using ShareFlow.Application.Revenue.Interfaces;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Events;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Application.Revenue;

public class RevenueService(
    IPlatformRevenueRepository revenueRepository,
    IVideoProjectRepository projectRepository,
    IAiRevenueSkillAgent aiAgent,
    IPlatformRevenueHandlerFactory handlerFactory,
    IRegionContext regionContext,
    IPublisher mediator,
    IMapper mapper) : IRevenueService
{
    public async Task<Guid> AddManualAsync(AddManualRevenueRequest req, Guid operatorId, CancellationToken ct = default)
    {
        _ = await projectRepository.GetByIdAsync(req.ProjectId, ct)
            ?? throw new BusinessException("项目不存在。", 404);

        var entity = PlatformRevenue.CreateManual(
            req.ProjectId, req.PlatformName, req.Amount,
            string.IsNullOrWhiteSpace(req.Currency) ? regionContext.DefaultCurrency : req.Currency,
            req.RevenueDate);

        await revenueRepository.AddAsync(entity, ct);
        return entity.Id;
    }

    public async Task<BatchImportResult> ImportCsvAsync(
        Guid projectId, string platform, Stream csvStream, CancellationToken ct = default)
    {
        _ = await projectRepository.GetByIdAsync(projectId, ct)
            ?? throw new BusinessException("项目不存在。", 404);

        var handler = handlerFactory.GetHandler(platform, RevenueImportSource.Csv);
        var request = new RevenueParseRequest { ProjectId = projectId, CsvStream = csvStream };

        List<ParsedRevenueItem> parsed;
        var errors = new List<string>();
        try
        {
            parsed = (await handler.ParseAsync(request, ct)).ToList();
        }
        catch (Exception ex)
        {
            return new BatchImportResult { Imported = 0, Failed = 1, Errors = [ex.Message] };
        }

        var entities = parsed.Select(item =>
            PlatformRevenue.CreateFromCsv(projectId, platform, item.Amount, item.Currency, item.RevenueDate)
        ).ToList();

        await revenueRepository.AddRangeAsync(entities, ct);
        return new BatchImportResult { Imported = entities.Count, Failed = errors.Count, Errors = errors };
    }

    public async Task<AiImportPreviewDto> PreviewScreenshotAsync(
        Guid projectId, string platform, string imageBase64, string mimeType, CancellationToken ct = default)
    {
        _ = await projectRepository.GetByIdAsync(projectId, ct)
            ?? throw new BusinessException("项目不存在。", 404);

        var result = await aiAgent.ExtractAsync(imageBase64, mimeType, platform, ct);

        // 持久化草稿以便后续确认
        var entity = PlatformRevenue.CreateFromAi(
            projectId, platform, result.Amount, result.Currency, result.Date,
            screenshotPath: null, aiRawResult: result.RawJson, aiConfidence: result.Confidence);

        await revenueRepository.AddAsync(entity, ct);

        return new AiImportPreviewDto
        {
            RevenueId = entity.Id,
            Amount = entity.Amount,
            Currency = entity.Currency,
            RevenueDate = entity.RevenueDate,
            Confidence = entity.AiConfidence,
            NeedsVerification = entity.Status == RevenueStatus.NeedsVerification,
            AiRawResult = entity.AiRawResult,
        };
    }

    public async Task ConfirmAiImportAsync(Guid revenueId, decimal? overrideAmount, CancellationToken ct = default)
    {
        var entity = await revenueRepository.GetByIdAsync(revenueId, ct)
            ?? throw new BusinessException("收益记录不存在。", 404);

        if (overrideAmount.HasValue)
            entity.OverrideAmount(overrideAmount.Value);

        // 确认后置为 Pending 等待审核
        if (entity.Status == RevenueStatus.NeedsVerification)
            entity.FlagForVerification(); // 保持待核实状态，审核人再决定

        await revenueRepository.UpdateAsync(entity, ct);
    }

    public async Task ApproveAsync(Guid revenueId, Guid verifierId, CancellationToken ct = default)
    {
        var entity = await revenueRepository.GetByIdAsync(revenueId, ct)
            ?? throw new BusinessException("收益记录不存在。", 404);

        if (entity.Status == RevenueStatus.Approved)
            throw new BusinessException("该收益已经审核通过。");

        entity.Approve(verifierId);
        await revenueRepository.UpdateAsync(entity, ct);

        await mediator.Publish(
            new RevenueApprovedEvent(entity.Id, entity.ProjectId, entity.Amount, entity.Currency, entity.RevenueDate), ct);
    }

    public async Task RejectAsync(Guid revenueId, Guid verifierId, string reason, CancellationToken ct = default)
    {
        var entity = await revenueRepository.GetByIdAsync(revenueId, ct)
            ?? throw new BusinessException("收益记录不存在。", 404);

        entity.Reject(verifierId, reason);
        await revenueRepository.UpdateAsync(entity, ct);
    }

    public async Task<PagedResult<RevenueDto>> GetListAsync(RevenueQueryRequest query, CancellationToken ct = default)
    {
        var paged = await revenueRepository.GetPagedAsync(
            query.ProjectId, query.Status, query.PlatformName, query.Page, query.PageSize, ct);

        return PagedResult<RevenueDto>.MapFrom(paged, query.Page, query.PageSize,
            items => mapper.Map<List<RevenueDto>>(items));
    }
}
