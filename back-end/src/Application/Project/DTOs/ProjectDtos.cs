using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Application.Project.DTOs;

// ───────── Query ─────────

/// <summary>
/// 项目列表查询参数。继承 PagedQuery 获得 Page/PageSize 边界校验。
/// </summary>
public record ProjectQueryRequest : PagedQuery
{
    public string? Title { get; init; }
    public ProjectStatus? Status { get; init; }
}

// ───────── Commands ─────────

public record CreateProjectRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string PlatformName { get; init; } = string.Empty;
    public ProjectSlotMode SlotMode { get; init; }
    public int TotalSlots { get; init; }
    public decimal? TotalInvestment { get; init; }
}

public record UpdateProjectRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string PlatformName { get; init; } = string.Empty;
    public int TotalSlots { get; init; }
    public decimal? TotalInvestment { get; init; }
}

public record ChangeProjectStatusRequest
{
    public ProjectStatus NewStatus { get; init; }
}

public record AddSlotRequest
{
    /// <summary>持股比例（百分比），如 3.5 代表 3.5%</summary>
    public decimal SharePct { get; init; }
}

public record UpdateSlotContractMonthsRequest
{
    /// <summary>合同期限（月），允许范围 1～120</summary>
    public int ContractMonths { get; init; }
}

public record BatchUpdateSlotsRequest
{
    /// <summary>要批量修改的槽位 ID 列表</summary>
    public IList<Guid> SlotIds { get; init; } = [];
    /// <summary>合同期限（月），为 null 则不修改</summary>
    public int? ContractMonths { get; init; }
    /// <summary>模板类型，为 null 则不修改</summary>
    public string? TemplateType { get; init; }
    /// <summary>持股比例（百分比），为 null 则不修改</summary>
    public decimal? SharePct { get; init; }
}

// ───────── Responses ─────────

public record ProjectDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string PlatformName { get; init; } = string.Empty;
    public string SlotMode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int TotalSlots { get; init; }
    public int FilledSlots { get; init; }
    public int ReservedSlots { get; init; }
    public int AvailableSlots { get; init; }
    public decimal FundingProgressPct { get; init; }
    public DateTime CreatedAt { get; init; }
    /// <summary>仅内部可见。前端客户端页面不展示此字段。</summary>
    public decimal? TotalInvestment { get; init; }
}

public record ProjectDetailDto : ProjectDto
{
    public IReadOnlyList<SlotDto> Slots { get; init; } = [];
}

public record SlotDto
{
    public Guid Id { get; init; }
    public decimal SharePct { get; init; }
    public string Status { get; init; } = string.Empty;
    public Guid? ClientUserId { get; init; }
    public int ContractMonths { get; init; }
    public string TemplateType { get; init; } = "OverseasEnglish";
}
