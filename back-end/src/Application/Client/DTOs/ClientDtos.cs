namespace ShareFlow.Application.Client.DTOs;

// ── Dashboard ────────────────────────────────────────────────

public class ClientDashboardDto
{
    public decimal WalletBalance { get; set; }
    public decimal FrozenAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal TotalDividendReceived { get; set; }
    public IReadOnlyList<ClientProjectSummaryDto> Projects { get; set; } = [];
    public IReadOnlyList<ClientRecentDividendDto> RecentDividends { get; set; } = [];
}

public class ClientProjectSummaryDto
{
    public Guid ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public string PlatformName { get; set; } = string.Empty;
    /// <summary>持股千分比，例如 35 表示 3.5%</summary>
    public decimal SharePermille { get; set; }
    public decimal TotalDividendReceived { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string ContractStatus { get; set; } = string.Empty;
}

public class ClientRecentDividendDto
{
    public Guid Id { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public decimal DividendAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; }
}

// ── Dividends ────────────────────────────────────────────────

public class ClientDividendDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public string PlatformName { get; set; } = string.Empty;
    public decimal RevenueAmount { get; set; }
    public decimal SharePermille { get; set; }
    public decimal DividendAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; }
}

// ── Contracts ────────────────────────────────────────────────

public class ClientContractDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public string PlatformName { get; set; } = string.Empty;
    public decimal SharePermille { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime? SignedAt { get; set; }
    public bool HasPdf { get; set; }
}
