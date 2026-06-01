using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Application.Contracts.DTOs;

// ───────── Query ─────────

public record ContractQueryRequest : PagedQuery
{
    public Guid? ProjectId { get; init; }
    public ContractStatus? Status { get; init; }
    public string? ProjectTitle { get; init; }
}

// ───────── Commands ─────────

/// <summary>创建合同草稿请求（关联已有项目槽位和投资人）</summary>
public record CreateContractRequest
{
    public Guid ProjectId { get; init; }
    public Guid SlotId { get; init; }
    public Guid InvestorUserId { get; init; }
}

/// <summary>公开签约端点：投资人提交 Canvas base64 签名</summary>
public record SubmitSignatureRequest
{
    /// <summary>Canvas toDataURL('image/png') 导出的 base64 字符串</summary>
    public string SignatureDataUrl { get; init; } = string.Empty;
}

// ───────── Responses ─────────

public record ContractDto
{
    public Guid Id { get; init; }
    public int ContractNo { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectTitle { get; init; } = string.Empty;
    public Guid SlotId { get; init; }
    public Guid InvestorUserId { get; init; }
    public string InvestorName { get; init; } = string.Empty;
    public ContractStatus Status { get; init; }
    public string TemplateType { get; init; } = string.Empty;
    public DateTime? SignedAt { get; init; }
    public bool HasPdf { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>合同详情（含签约链接生成信息）</summary>
public record ContractDetailDto : ContractDto
{
    public string? SignUrl { get; init; }
    public DateTime? SignTokenExpiresAt { get; init; }
    public string? PdfStoragePath { get; init; }
    public string? ContractSnapshot { get; init; }
}

/// <summary>生成签约链接后返回给管理员的结果</summary>
public record GenerateSignLinkResult
{
    public int ContractNo { get; init; }
    public string SignUrl { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}

/// <summary>公开签约页预览数据（无需登录即可获取，通过 Token 验证）
/// </summary>
public record ContractPreviewDto
{
    public Guid ContractId { get; init; }
    public string ProjectTitle { get; init; } = string.Empty;
    public string InvestorName { get; init; } = string.Empty;
    public decimal SharePct { get; init; }
    public string TemplateType { get; init; } = string.Empty;
    public string? ContractSnapshot { get; init; }
    public DateTime ExpiresAt { get; init; }
}

/// <summary>签约完成后返回结果（包含初始账号信息）</summary>
public record SignContractResult
{
    /// <summary>初始用户名，仅首次判断时非 null</summary>
    public string? ClientUsername { get; init; }
    /// <summary>初始密码明文，仅首次判断时非 null，展示后即废弃</summary>
    public string? InitialPassword { get; init; }
}
