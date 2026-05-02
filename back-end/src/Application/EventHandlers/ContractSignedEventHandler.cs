using MediatR;
using ShareFlow.Application.Auth.Interfaces;
using ShareFlow.Application.Common.Interfaces;
using ShareFlow.Application.Contracts.Interfaces;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Events;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Application.EventHandlers;

/// <summary>
/// 合同签署后触发：
/// 1. 将 ProjectSlot 状态流转为 Occupied，绑定 InvestorUserId
/// 2. 用 QuestPDF 生成合同 PDF → 上传 MinIO → 更新 PdfStoragePath → 状态流转到 Executed
/// </summary>
public class ContractSignedEventHandler(
    IContractRepository contractRepository,
    IProjectSlotRepository slotRepository,
    IPdfGeneratorService pdfGenerator,
    IStorageService storageService) : INotificationHandler<ContractSignedEvent>
{
    public async Task Handle(ContractSignedEvent evt, CancellationToken ct)
    {
        // ── 1. 槽位状态流转 ────────────────────────────────────────────
        try
        {
            var slot = await slotRepository.GetByIdAsync(evt.SlotId, ct);
            if (slot is not null && slot.Status != SlotStatus.Occupied)
            {
                if (slot.Status == SlotStatus.Available)
                    slot.Reserve(evt.InvestorUserId);

                slot.Confirm();
                await slotRepository.UpdateAsync(slot, ct);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ContractSignedEventHandler] Slot update failed for slot {evt.SlotId}: {ex.Message}");
        }

        // ── 2. 生成 PDF 并上传 MinIO ──────────────────────────────────
        try
        {
            var pdfBytes   = await pdfGenerator.GenerateContractPdfAsync(evt.ContractId, ct);
            var objectName = $"contracts/{evt.ContractId}.pdf";

            await storageService.UploadAsync(objectName, pdfBytes, "application/pdf", ct);

            var contract = await contractRepository.GetByIdAsync(evt.ContractId, ct);
            if (contract is not null)
            {
                contract.PdfGenerated(objectName);   // 存储 objectName，而非本地路径
                await contractRepository.UpdateAsync(contract, ct);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ContractSignedEventHandler] PDF generation/upload failed for {evt.ContractId}: {ex.Message}");
        }
    }
}
