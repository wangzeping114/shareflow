namespace ShareFlow.Application.Revenue.Interfaces;

public interface IAiRevenueSkillAgent
{
    Task<AiRevenueExtractResult> ExtractAsync(string imageBase64, string mimeType, string platform, CancellationToken ct = default);
}

public record AiRevenueExtractResult
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public decimal Confidence { get; init; }
    public string RawJson { get; init; } = string.Empty;
}
