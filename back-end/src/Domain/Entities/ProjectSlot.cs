using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Entities;

public class ProjectSlot : Entity<Guid>
{
    public Guid ProjectId { get; private set; }

    /// <summary>项目内槽位序号，从 1 开始自动递增</summary>
    public int SlotNumber { get; private set; }

    /// <summary>可选别名，例如 "A座" / "VIP-1"（最长 50 字符）</summary>
    public string? Alias { get; private set; }

    /// <summary>持股千分比，例如 35 表示 3.5%（decimal(8,4) 存储时为 3.5000）</summary>
    public decimal SharePermille { get; private set; }

    public SlotStatus Status { get; private set; }

    public Guid? ClientUserId { get; private set; }

    /// <summary>合同期限（月），默认 12 个月</summary>
    public int ContractMonths { get; private set; } = 12;

    /// <summary>合同模板类型，默认 OverseasEnglish</summary>
    public string TemplateType { get; private set; } = "OverseasEnglish";

    private ProjectSlot() { }

    public static ProjectSlot Create(Guid projectId, decimal sharePermille, int slotNumber)
    {
        if (sharePermille <= 0) throw new ArgumentException("持股比例必须大于 0。", nameof(sharePermille));
        if (slotNumber < 1) throw new ArgumentException("槽位序号必须大于 0。", nameof(slotNumber));

        return new ProjectSlot
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            SlotNumber = slotNumber,
            SharePermille = sharePermille,
            Status = SlotStatus.Available,
            ContractMonths = 12,
            TemplateType = "OverseasEnglish"
        };
    }

    public void SetAlias(string? alias)
    {
        if (alias is not null && alias.Length > 50)
            throw new ArgumentException("别名最长 50 个字符。", nameof(alias));
        Alias = string.IsNullOrWhiteSpace(alias) ? null : alias.Trim();
        SetUpdatedAt();
    }

    public void SetContractMonths(int months)
    {
        if (Status == SlotStatus.Occupied)
            throw new InvalidOperationException("已签约的槽位不能修改合同期限。");
        if (months < 1 || months > 120)
            throw new ArgumentOutOfRangeException(nameof(months), "合同期限应在 1～120 个月之间。");
        ContractMonths = months;
        SetUpdatedAt();
    }

    public void SetSharePermille(decimal sharePermille)
    {
        if (Status == SlotStatus.Occupied)
            throw new InvalidOperationException("已签约的槽位不能修改持股比例。");
        if (sharePermille <= 0)
            throw new ArgumentException("持股比例必须大于 0。", nameof(sharePermille));
        SharePermille = sharePermille;
        SetUpdatedAt();
    }

    public void SetTemplateType(string templateType)
    {
        if (Status == SlotStatus.Occupied)
            throw new InvalidOperationException("已签约的槽位不能修改合同模板。");
        var allowed = new[] { "OverseasEnglish", "DomesticChinese" };
        if (!allowed.Contains(templateType))
            throw new ArgumentException($"模板类型不合法：{templateType}", nameof(templateType));
        TemplateType = templateType;
        SetUpdatedAt();
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
        Status = SlotStatus.Available;
        SetUpdatedAt();
    }
}
