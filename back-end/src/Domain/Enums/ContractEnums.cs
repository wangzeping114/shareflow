namespace ShareFlow.Domain.Enums;

public enum ContractStatus
{
    Draft,          // 草稿（已创建，尚未发送）
    Sent,           // 已发送（等待投资人签署）
    Signed,         // 已签署（待生成 PDF）
    Executed,       // 已执行（PDF 生成完毕）
    Expired,        // 链接已过期
    PendingRenew,   // 等待续签审批
    Renewing,       // 续签中
    Superseded      // 已被新合同取代
}
