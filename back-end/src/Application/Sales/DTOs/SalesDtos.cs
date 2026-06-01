using ShareFlow.Domain.Enums;

namespace ShareFlow.Application.Sales.DTOs;

// ───────── Leads ─────────

public record LeadDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ContactInfo { get; init; } = string.Empty;
    public string? Email { get; init; }
    public LeadStatus Status { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record CreateLeadRequest
{
    public string Name { get; init; } = string.Empty;
    public string ContactInfo { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Notes { get; init; }
}

public record UpdateLeadRequest
{
    public string Name { get; init; } = string.Empty;
    public string ContactInfo { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Notes { get; init; }
    public LeadStatus Status { get; init; }
}

// ───────── Projects ─────────

public record SalesProjectDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string PlatformName { get; init; } = string.Empty;
    public int AvailableSlots { get; init; }
    public IReadOnlyList<SalesProjectSlotDto> Slots { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}

public record SalesProjectSlotDto
{
    public Guid Id { get; init; }
    public int SlotNumber { get; init; }
    public string? Alias { get; init; }
    public decimal SharePermille { get; init; }
}

public record SalesClientDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    /// <summary>该线索小客户是否已创建登录账号</summary>
    public bool HasAccount { get; init; }
}

// ───────── Contracts ─────────

public record InitiateContractRequest
{
    public Guid ProjectId { get; init; }
    public Guid SlotId { get; init; }
    public Guid InvestorUserId { get; init; }
}

public record ChangeContractClientRequest
{
    public Guid NewClientId { get; init; }
}

public record InitiateContractResult
{
    public Guid ContractId { get; init; }
    public int ContractNo { get; init; }
    public string SignUrl { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    /// <summary>仅首次创建账号时非 null</summary>
    public string? ClientUsername { get; init; }
    /// <summary>仅首次创建账号时非 null，明文临时密码，请立即告知客户</summary>
    public string? TempPassword { get; init; }
}

public record SalesContractDetailDto
{
    public Guid Id { get; init; }
    public int ContractNo { get; init; }
    public string ProjectTitle { get; init; } = string.Empty;
    public string ClientName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal SharePermille { get; init; }
    public string? ContractSnapshot { get; init; }
    public string? SignatureDataUrl { get; init; }
    public DateTime? SignedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    /// <summary>客户登录用户名</summary>
    public string? ClientUsername { get; init; }
    /// <summary>初始密码（存储在 Lead 上，销售侧永久可见）</summary>
    public string? ClientInitialPassword { get; init; }
    /// <summary>签约链接（仅 Draft/Sent 状态且 Token 未过期时有值）</summary>
    public string? SignUrl { get; init; }
    /// <summary>签约链接过期时间</summary>
    public DateTime? SignTokenExpiresAt { get; init; }
}

public record SalesContractDto
{
    public Guid Id { get; init; }
    public int ContractNo { get; init; }
    public string ProjectTitle { get; init; } = string.Empty;
    public Guid SlotId { get; init; }
    public int SlotNumber { get; init; }
    public string? SlotAlias { get; init; }
    public decimal SharePermille { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public ContractStatus Status { get; init; }
    public DateTime? SignedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

// ───────── Performance ─────────

public record SalesPerformanceDto
{
    public int TotalLeads { get; init; }
    public int ConvertedLeads { get; init; }
    public decimal ConversionRate { get; init; }
    public int ContractsSent { get; init; }
    public int ContractsSigned { get; init; }
}

// ───────── Query ─────────

public record LeadQueryRequest
{
    public LeadStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
