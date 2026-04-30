namespace ShareFlow.Domain.Enums;

public enum DividendStatus
{
    Calculated,   // 已计算，等待确认
    Confirmed,    // 已确认，等待分发
    Distributed   // 已分发（钱包已到账）
}
