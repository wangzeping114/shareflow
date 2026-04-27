using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Entities;

public class ProjectSlot : Entity<Guid>
{
    public Guid ProjectId { get; private set; }

    /// <summary>持股千分比，例如 35 表示 3.5%（decimal(8,4) 存储时为 3.5000）</summary>
    public decimal SharePermille { get; private set; }

    public SlotStatus Status { get; private set; }

    public Guid? ClientUserId { get; private set; }

    private ProjectSlot() { }

    public static ProjectSlot Create(Guid projectId, decimal sharePermille)
    {
        if (sharePermille <= 0) throw new ArgumentException("持股比例必须大于 0。", nameof(sharePermille));

        return new ProjectSlot
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            SharePermille = sharePermille,
            Status = SlotStatus.Available
        };
    }

    public void Reserve(Guid clientUserId)
    {
        if (Status != SlotStatus.Available)
            throw new InvalidOperationException("该槽位当前不可预留。");

        ClientUserId = clientUserId;
        Status = SlotStatus.Reserved;
        SetUpdatedAt();
    }

    public void Confirm()
    {
        if (Status != SlotStatus.Reserved)
            throw new InvalidOperationException("只有处于预留状态的槽位才能确认。");

        Status = SlotStatus.Occupied;
        SetUpdatedAt();
    }

    public void Release()
    {
        ClientUserId = null;
        Status = SlotStatus.Released;
        SetUpdatedAt();
    }
}
