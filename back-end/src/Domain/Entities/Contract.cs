using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Entities;

public class Contract : Entity<Guid>
{
    public Guid ProjectId { get; private set; }
    public Guid SlotId { get; private set; }
    public Guid InvestorUserId { get; private set; }
    public ContractStatus Status { get; private set; }

    /// <summary>合同模板类型，由 IRegionContext 决定（OverseasEnglish / DomesticChinese）</summary>
    public string TemplateType { get; private set; } = string.Empty;

    /// <summary>签约短链接 Token（UUID，有效期 7 天）</summary>
    public string? SignToken { get; private set; }
    public DateTime? SignTokenExpiresAt { get; private set; }

    /// <summary>Canvas base64 签名图像</summary>
    public string? SignatureDataUrl { get; private set; }
    public DateTime? SignedAt { get; private set; }

    /// <summary>PDF 存储路径（本地文件系统或对象存储）</summary>
    public string? PdfStoragePath { get; private set; }

    /// <summary>合同描述快照（项目名称 + 份额信息，便于历史查阅）</summary>
    public string? ContractSnapshot { get; private set; }

    // 导航属性
    public VideoProject? Project { get; private set; }
    public ProjectSlot? Slot { get; private set; }
    public User? InvestorUser { get; private set; }

    private Contract() { }

    public static Contract Create(
        Guid projectId,
        Guid slotId,
        Guid investorUserId,
        string templateType,
        string? contractSnapshot = null)
    {
        return new Contract
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            SlotId = slotId,
            InvestorUserId = investorUserId,
            TemplateType = templateType,
            ContractSnapshot = contractSnapshot,
            Status = ContractStatus.Draft,
        };
    }

    /// <summary>生成签约 Token，状态流转为 Sent。返回 Token 字符串。</summary>
    public string GenerateSignToken()
    {
        SignToken = Guid.NewGuid().ToString("N");
        SignTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        Status = ContractStatus.Sent;
        SetUpdatedAt();
        return SignToken;
    }

    /// <summary>投资人提交 Canvas 签名，状态流转为 Signed。</summary>
    public void Sign(string signatureDataUrl)
    {
        if (Status != ContractStatus.Sent)
            throw new BusinessException("合同当前状态不允许签署。", 400);

        if (SignTokenExpiresAt.HasValue && SignTokenExpiresAt.Value < DateTime.UtcNow)
        {
            Status = ContractStatus.Expired;
            SetUpdatedAt();
            throw new BusinessException("签约链接已过期，请联系销售人员重新发送。", 400);
        }

        SignatureDataUrl = signatureDataUrl;
        SignedAt = DateTime.UtcNow;
        Status = ContractStatus.Signed;
        SetUpdatedAt();
    }

    /// <summary>PDF 生成完毕，更新路径并流转为 Executed。</summary>
    public void PdfGenerated(string storagePath)
    {
        PdfStoragePath = storagePath;
        Status = ContractStatus.Executed;
        SetUpdatedAt();
    }

    /// <summary>签约完成后嵌入签名日期到快照</summary>
    public void UpdateSnapshot(string newSnapshot)
    {
        ContractSnapshot = newSnapshot;
        SetUpdatedAt();
    }

    /// <summary>标记为已过期。</summary>
    public void MarkExpired()
    {
        Status = ContractStatus.Expired;
        SetUpdatedAt();
    }

    /// <summary>被新合同取代（续签场景）。</summary>
    public void Supersede()
    {
        Status = ContractStatus.Superseded;
        SetUpdatedAt();
    }

    /// <summary>撤销合同（仅允许 Draft / Sent 状态）。</summary>
    public void Cancel()
    {
        if (Status == ContractStatus.Signed || Status == ContractStatus.Executed)
            throw new BusinessException("已签署/执行的合同无法撤销。", 400);
        Status = ContractStatus.Expired;
        SignToken = null;
        SignTokenExpiresAt = null;
        SetUpdatedAt();
    }

    /// <summary>更换持股人（仅允许 Draft / Sent 状态）。</summary>
    public void ChangeInvestor(Guid newInvestorUserId)
    {
        if (Status == ContractStatus.Signed || Status == ContractStatus.Executed)
            throw new BusinessException("已签署/执行的合同无法更换持股人。", 400);
        InvestorUserId = newInvestorUserId;
        // 重置 Token，防止旧链接被新客户使用
        SignToken = null;
        SignTokenExpiresAt = null;
        Status = ContractStatus.Draft;
        SetUpdatedAt();
    }
}
