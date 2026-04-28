using ShareFlow.Domain.Common;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Tests;

/// <summary>
/// Contract 实体状态机单元测试。
/// 遵循命名规范：MethodName_StateUnderTest_ExpectedBehavior
/// </summary>
public class ContractEntityTests
{
    // ──────────────────────── Create ────────────────────────

    [Fact]
    public void Create_ValidArguments_ReturnsContractInDraftStatus()
    {
        var projectId = Guid.NewGuid();
        var slotId = Guid.NewGuid();
        var investorId = Guid.NewGuid();

        var contract = Contract.Create(projectId, slotId, investorId, "DomesticChinese", "snapshot");

        Assert.Equal(ContractStatus.Draft, contract.Status);
        Assert.Equal(projectId, contract.ProjectId);
        Assert.Equal(slotId, contract.SlotId);
        Assert.Equal(investorId, contract.InvestorUserId);
        Assert.Equal("DomesticChinese", contract.TemplateType);
        Assert.Equal("snapshot", contract.ContractSnapshot);
        Assert.NotEqual(Guid.Empty, contract.Id);
    }

    // ──────────────────────── GenerateSignToken ────────────────────────

    [Fact]
    public void GenerateSignToken_DraftContract_StatusBecomesSent()
    {
        var contract = MakeDraftContract();

        var token = contract.GenerateSignToken();

        Assert.Equal(ContractStatus.Sent, contract.Status);
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(token, contract.SignToken);
        Assert.NotNull(contract.SignTokenExpiresAt);
        Assert.True(contract.SignTokenExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public void GenerateSignToken_CalledTwice_OverwritesPreviousToken()
    {
        var contract = MakeDraftContract();
        var first = contract.GenerateSignToken();
        var second = contract.GenerateSignToken();

        Assert.NotEqual(first, second);
        Assert.Equal(second, contract.SignToken);
    }

    // ──────────────────────── Sign ────────────────────────

    [Fact]
    public void Sign_SentContract_StatusBecomesSignedAndSetsSignature()
    {
        var contract = MakeSentContract();

        contract.Sign("data:image/png;base64,AAAA");

        Assert.Equal(ContractStatus.Signed, contract.Status);
        Assert.Equal("data:image/png;base64,AAAA", contract.SignatureDataUrl);
        Assert.NotNull(contract.SignedAt);
    }

    [Fact]
    public void Sign_WhenStatusIsDraft_ThrowsBusinessException()
    {
        var contract = MakeDraftContract();

        var ex = Assert.Throws<BusinessException>(() => contract.Sign("data:image/png;base64,AAAA"));
        Assert.Equal(400, ex.Code);
    }

    [Fact]
    public void Sign_ExpiredToken_ThrowsBusinessExceptionAndStatusBecomesExpired()
    {
        var contract = MakeSentContract(tokenExpiresAt: DateTime.UtcNow.AddSeconds(-1));

        var ex = Assert.Throws<BusinessException>(() => contract.Sign("data:image/png;base64,AAAA"));

        Assert.Equal(400, ex.Code);
        Assert.Equal(ContractStatus.Expired, contract.Status);
    }

    // ──────────────────────── PdfGenerated ────────────────────────

    [Fact]
    public void PdfGenerated_SignedContract_StatusBecomesExecuted()
    {
        var contract = MakeSignedContract();

        contract.PdfGenerated("/storage/contracts/test.pdf");

        Assert.Equal(ContractStatus.Executed, contract.Status);
        Assert.Equal("/storage/contracts/test.pdf", contract.PdfStoragePath);
    }

    // ──────────────────────── MarkExpired ────────────────────────

    [Fact]
    public void MarkExpired_AnyContract_StatusBecomesExpired()
    {
        var contract = MakeSentContract();

        contract.MarkExpired();

        Assert.Equal(ContractStatus.Expired, contract.Status);
    }

    // ──────────────────────── Supersede ────────────────────────

    [Fact]
    public void Supersede_ExecutedContract_StatusBecomesSuperseded()
    {
        var contract = MakeSignedContract();
        contract.PdfGenerated("/path/old.pdf");

        contract.Supersede();

        Assert.Equal(ContractStatus.Superseded, contract.Status);
    }

    // ──────────────────────── Helpers ────────────────────────

    private static Contract MakeDraftContract()
        => Contract.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "DomesticChinese");

    private static Contract MakeSentContract(DateTime? tokenExpiresAt = null)
    {
        var contract = MakeDraftContract();
        contract.GenerateSignToken();

        // 若需要自定义过期时间，通过反射写入（测试专用）
        if (tokenExpiresAt.HasValue)
        {
            typeof(Contract)
                .GetProperty(nameof(Contract.SignTokenExpiresAt))!
                .SetValue(contract, tokenExpiresAt.Value);
        }

        return contract;
    }

    private static Contract MakeSignedContract()
    {
        var contract = MakeSentContract();
        contract.Sign("data:image/png;base64,AAAA");
        return contract;
    }
}
