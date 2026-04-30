using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Entities;

public class DividendRecord : Entity<Guid>
{
    public Guid ProjectId { get; private set; }
    public Guid SlotId { get; private set; }
    public Guid InvestorUserId { get; private set; }
    public Guid PlatformRevenueId { get; private set; }

    /// <summary>参与分配的收益额（快照）</summary>
    public decimal RevenueAmount { get; private set; }

    /// <summary>持股千分比快照，例如 35 表示 3.5%</summary>
    public decimal SharePermille { get; private set; }

    /// <summary>分红金额 = RevenueAmount * SharePermille / 1000，保留 2 位小数</summary>
    public decimal DividendAmount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public DividendStatus Status { get; private set; }

    public DateTime CalculatedAt { get; private set; }

    // 导航属性
    public VideoProject? Project { get; private set; }
    public ProjectSlot? Slot { get; private set; }
    public User? InvestorUser { get; private set; }
    public PlatformRevenue? PlatformRevenue { get; private set; }

    private DividendRecord() { }

    public static DividendRecord Calculate(
        Guid projectId,
        Guid slotId,
        Guid investorUserId,
        Guid revenueId,
        decimal revenueAmount,
        decimal sharePermille,
        string currency)
    {
        return new DividendRecord
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            SlotId = slotId,
            InvestorUserId = investorUserId,
            PlatformRevenueId = revenueId,
            RevenueAmount = revenueAmount,
            SharePermille = sharePermille,
            DividendAmount = Math.Round(revenueAmount * sharePermille / 1000m, 2),
            Currency = currency,
            Status = DividendStatus.Calculated,
            CalculatedAt = DateTime.UtcNow,
        };
    }

    public void Confirm()
    {
        Status = DividendStatus.Confirmed;
        SetUpdatedAt();
    }

    public void MarkDistributed()
    {
        Status = DividendStatus.Distributed;
        SetUpdatedAt();
    }
}
