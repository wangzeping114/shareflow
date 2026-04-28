using MediatR;
using ShareFlow.Application.Auth.Interfaces;
using ShareFlow.Application.Contracts.Interfaces;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Events;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Application.EventHandlers;

/// <summary>
/// 合同签署后触发：
/// 1. 生成合同 PDF
/// 2. 更新 PdfStoragePath → 状态流转到 Executed
/// （Epic 11 可扩展：推站内通知）
/// </summary>
public class ContractSignedEventHandler(
    IContractRepository contractRepository,
    IPdfGeneratorService pdfGenerator) : INotificationHandler<ContractSignedEvent>
{
    public async Task Handle(ContractSignedEvent evt, CancellationToken ct)
    {
        try
        {
            // 生成 PDF
            var pdfBytes = await pdfGenerator.GenerateContractPdfAsync(evt.ContractId, ct);

            // 本地存储（简单实现；生产可替换为 S3/OSS）
            var storagePath = $"contracts/{evt.ContractId}.pdf";
            var dir = Path.GetDirectoryName(storagePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            await File.WriteAllBytesAsync(storagePath, pdfBytes, ct);

            // 更新合同状态
            var contract = await contractRepository.GetByIdAsync(evt.ContractId, ct);
            if (contract is not null)
            {
                contract.PdfGenerated(storagePath);
                await contractRepository.UpdateAsync(contract, ct);
            }
        }
        catch (Exception ex)
        {
            // 日志记录但不抛出，避免阻断签约主流程
            // 生产环境应接入 Serilog 结构化日志
            Console.Error.WriteLine($"[ContractSignedEventHandler] PDF generation failed for {evt.ContractId}: {ex.Message}");
        }
    }
}
