using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Events;

namespace ShareFlow.Domain.Entities;

public class PlatformRevenue : Entity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string PlatformName { get; private set; } = string.Empty;

    /// <summary>原始平台货币金额</summary>
    public decimal Amount { get; private set; }

    /// <summary>货币代码，如 USD / CNY</summary>
    public string Currency { get; private set; } = string.Empty;

    /// <summary>收益所属日期（UTC 日期部分）</summary>
    public DateTime RevenueDate { get; private set; }

    public RevenueImportSource ImportSource { get; private set; }
    public RevenueStatus Status { get; private set; } = RevenueStatus.Pending;

    /// <summary>AI 或 CSV 导入时截图/文件存储路径</summary>
    public string? ScreenshotStoragePath { get; private set; }

    /// <summary>AI 原始返回 JSON</summary>
    public string? AiRawResult { get; private set; }

    /// <summary>AI 置信度 0-1</summary>
    public decimal AiConfidence { get; private set; }

    public Guid? VerifiedByUserId { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public string? RejectReason { get; private set; }

    // 导航属性
    public VideoProject? Project { get; private set; }

    private PlatformRevenue() { }

    public static PlatformRevenue CreateManual(
        Guid projectId, string platformName, decimal amount, string currency, DateTime revenueDate)
    {
        return new PlatformRevenue
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            PlatformName = platformName,
            Amount = amount,
            Currency = currency,
            RevenueDate = revenueDate.Date,
            ImportSource = RevenueImportSource.Manual,
            Status = RevenueStatus.Pending,
        };
    }

    public static PlatformRevenue CreateFromCsv(
        Guid projectId, string platformName, decimal amount, string currency, DateTime revenueDate)
    {
        return new PlatformRevenue
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            PlatformName = platformName,
            Amount = amount,
            Currency = currency,
            RevenueDate = revenueDate.Date,
            ImportSource = RevenueImportSource.Csv,
            Status = RevenueStatus.Pending,
        };
    }

    public static PlatformRevenue CreateFromAi(
        Guid projectId, string platformName, decimal amount, string currency, DateTime revenueDate,
        string? screenshotPath, string? aiRawResult, decimal aiConfidence)
    {
        var status = aiConfidence < 0.7m ? RevenueStatus.NeedsVerification : RevenueStatus.Pending;
        return new PlatformRevenue
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            PlatformName = platformName,
            Amount = amount,
            Currency = currency,
            RevenueDate = revenueDate.Date,
            ImportSource = RevenueImportSource.AiScreenshot,
            Status = status,
            ScreenshotStoragePath = screenshotPath,
            AiRawResult = aiRawResult,
            AiConfidence = aiConfidence,
        };
    }

    public void OverrideAmount(decimal newAmount)
    {
        Amount = newAmount;
        SetUpdatedAt();
    }

    public void Approve(Guid verifierId)
    {
        Status = RevenueStatus.Approved;
        VerifiedByUserId = verifierId;
        VerifiedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Reject(Guid verifierId, string reason)
    {
        Status = RevenueStatus.Rejected;
        VerifiedByUserId = verifierId;
        VerifiedAt = DateTime.UtcNow;
        RejectReason = reason;
        SetUpdatedAt();
    }

    public void FlagForVerification()
    {
        Status = RevenueStatus.NeedsVerification;
        SetUpdatedAt();
    }
}
