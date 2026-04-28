using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Application.Revenue.DTOs;

// ───── Query ─────

public record RevenueQueryRequest : PagedQuery
{
    public Guid? ProjectId { get; init; }
    public RevenueStatus? Status { get; init; }
    public string? PlatformName { get; init; }
}

// ───── Commands ─────

public record AddManualRevenueRequest
{
    public Guid ProjectId { get; init; }
    public string PlatformName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime RevenueDate { get; init; }
}

public record ConfirmAiImportRequest
{
    public decimal? OverrideAmount { get; init; }
}

public record RejectRevenueRequest
{
    public string Reason { get; init; } = string.Empty;
}

// ───── Responses ─────

public record RevenueDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectTitle { get; init; } = string.Empty;
    public string PlatformName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime RevenueDate { get; init; }
    public string ImportSource { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal AiConfidence { get; init; }
    public string? RejectReason { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AiImportPreviewDto
{
    public Guid RevenueId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime RevenueDate { get; init; }
    public decimal Confidence { get; init; }
    public bool NeedsVerification { get; init; }
    public string? AiRawResult { get; init; }
}

public record ParsedRevenueItem
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime RevenueDate { get; init; }
}

public record BatchImportResult
{
    public int Imported { get; init; }
    public int Failed { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
}

public record RevenueParseRequest
{
    public Guid ProjectId { get; init; }
    public Stream? CsvStream { get; init; }
    public string? ImageBase64 { get; init; }
    public string? MimeType { get; init; }
}
