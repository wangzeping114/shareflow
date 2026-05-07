using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Entities;

public class VideoProject : Entity<Guid>
{
    private readonly List<ProjectSlot> _slots = [];

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string PlatformName { get; private set; } = string.Empty;
    public ProjectSlotMode SlotMode { get; private set; }
    public int TotalSlots { get; private set; }
    public ProjectStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    /// <summary>总投资金额（仅内部可见，不对客户展示）</summary>
    public decimal? TotalInvestment { get; private set; }

    // 导航属性（只读）
    public IReadOnlyList<ProjectSlot> Slots => _slots.AsReadOnly();

    // 计算属性
    public int FilledSlots => _slots.Count(s => s.Status == SlotStatus.Occupied);
    public int ReservedSlots => _slots.Count(s => s.Status == SlotStatus.Reserved);
    public int AvailableSlots => TotalSlots - FilledSlots - ReservedSlots;
    public bool HasAvailableSlot => AvailableSlots > 0;

    private VideoProject() { }

    public static VideoProject Create(
        string title,
        string description,
        string platformName,
        ProjectSlotMode slotMode,
        int totalSlots,
        Guid createdByUserId,
        decimal? totalInvestment = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        if (totalSlots <= 0) throw new ArgumentException("总份数必须大于 0。", nameof(totalSlots));

        return new VideoProject
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            PlatformName = platformName,
            SlotMode = slotMode,
            TotalSlots = totalSlots,
            Status = ProjectStatus.Draft,
            CreatedByUserId = createdByUserId,
            TotalInvestment = totalInvestment
        };
    }

    public void Update(string title, string description, string platformName, int totalSlots, decimal? totalInvestment = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        if (totalSlots < FilledSlots + ReservedSlots)
            throw new InvalidOperationException("总份数不能低于已占用与已预留份数之和。");

        Title = title;
        Description = description;
        PlatformName = platformName;
        TotalSlots = totalSlots;
        TotalInvestment = totalInvestment;
        SetUpdatedAt();
    }

    public void Activate()
    {
        Status = ProjectStatus.Active;
        SetUpdatedAt();
    }

    public void Pause()
    {
        Status = ProjectStatus.Paused;
        SetUpdatedAt();
    }

    public void Close()
    {
        Status = ProjectStatus.Closed;
        SetUpdatedAt();
    }

    /// <summary>重新开启已关闭的项目，回到草稿状态进行重新配置。</summary>
    public void Reopen()
    {
        Status = ProjectStatus.Draft;
        SetUpdatedAt();
    }

    /// <summary>在 Fixed 模式下，创建槽位时每份持股比例由调用方指定。</summary>
    public ProjectSlot AddSlot(decimal sharePermille)
    {
        if (_slots.Count >= TotalSlots)
            throw new InvalidOperationException("已达到总份数上限，无法添加更多槽位。");

        var slot = ProjectSlot.Create(Id, sharePermille, _slots.Count + 1);
        _slots.Add(slot);
        SetUpdatedAt();
        return slot;
    }

    /// <summary>EF Core 使用，用于加载已持久化的 Slots 集合。</summary>
    public void SetSlots(IEnumerable<ProjectSlot> slots)
    {
        _slots.Clear();
        _slots.AddRange(slots);
    }
}
