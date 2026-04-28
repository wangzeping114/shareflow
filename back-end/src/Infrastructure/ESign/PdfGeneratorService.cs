using ShareFlow.Application.Contracts.Interfaces;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Infrastructure.ESign;

/// <summary>PDF 生成服务（Stub 实现，返回最小化 PDF bytes）</summary>
public class PdfGeneratorService(IContractRepository contractRepo) : IPdfGeneratorService
{
    public async Task<byte[]> GenerateContractPdfAsync(Guid contractId, CancellationToken ct = default)
    {
        var contract = await contractRepo.GetByIdWithDetailsAsync(contractId, ct);

        // Stub: 返回简单的 PDF 占位内容（生产环境替换为 QuestPDF 模板实现）
        var content = $"""
            %PDF-1.4
            ShareFlow Contract
            ID: {contractId}
            Project: {contract?.Project?.Title ?? "N/A"}
            Investor: {contract?.InvestorUser?.DisplayName ?? "N/A"}
            Signed At: {contract?.SignedAt:yyyy-MM-dd HH:mm:ss} UTC
            """;

        return System.Text.Encoding.UTF8.GetBytes(content);
    }
}
