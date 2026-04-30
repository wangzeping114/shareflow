using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Application.Dividend.DTOs;

// ───── Query ─────

public record DividendQueryRequest : PagedQuery
{
    /// <summary>支持多项目筛选，为空时返回所有项目</summary>
    public IReadOnlyList<Guid>? ProjectIds { get; init; }
    /// <summary>支持多状态筛选，为空时返回所有状态</summary>
    public IReadOnlyList<DividendStatus>? Statuses { get; init; }
    public Guid? InvestorUserId { get; init; }
}

// ───── Responses ─────

public record DividendDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectTitle { get; init; } = string.Empty;
    public Guid SlotId { get; init; }
    public Guid InvestorUserId { get; init; }
    public string InvestorName { get; init; } = string.Empty;
    public Guid PlatformRevenueId { get; init; }
    public decimal RevenueAmount { get; init; }
    public decimal SharePermille { get; init; }
    public decimal DividendAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CalculatedAt { get; init; }
}

public record DividendProjectSummaryDto
{
    public Guid ProjectId { get; init; }
    public decimal TotalCalculated { get; init; }
    public decimal TotalConfirmed { get; init; }
    public decimal TotalDistributed { get; init; }
    public int RecordCount { get; init; }
    public string Currency { get; init; } = string.Empty;
}

// ───── Commands ─────

public record BatchDividendRequest
{
    public IReadOnlyList<Guid> DividendIds { get; init; } = [];
}
