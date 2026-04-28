using ShareFlow.Application.Contracts.DTOs;

namespace ShareFlow.Application.Contracts.Interfaces;

public interface IESignService
{
    /// <summary>验证 Token 有效性，返回合同预览数据（公开端点，无需登录）</summary>
    Task<ContractPreviewDto> GetPreviewAsync(string token, CancellationToken ct = default);

    /// <summary>投资人提交 Canvas 签名并完成签约</summary>
    Task SignAsync(string token, string signatureDataUrl, CancellationToken ct = default);
}

public interface IPdfGeneratorService
{
    /// <summary>生成合同 PDF，返回文件字节数组</summary>
    Task<byte[]> GenerateContractPdfAsync(Guid contractId, CancellationToken ct = default);
}
