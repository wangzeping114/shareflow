namespace ShareFlow.Domain.Enums;

public enum ProjectSlotMode
{
    Fixed,      // 固定份数：每份持股比例相同
    Flexible    // 灵活份数：每份持股比例由销售员填写
}

public enum ProjectStatus
{
    Draft,          // 草稿
    Active,         // 运营中（可签约）
    Paused,         // 暂停签约
    Closed          // 已关闭
}

public enum SlotStatus
{
    Available,  // 空位，可签约
    Reserved,   // 已预留（合同草稿阶段）
    Occupied,   // 已占用（合同 Active）
    Released    // 已释放（合同 Voided/Terminated）
}
