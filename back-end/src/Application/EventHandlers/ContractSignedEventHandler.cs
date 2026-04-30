using MediatR;
using ShareFlow.Application.Auth.Interfaces;
using ShareFlow.Application.Contracts.Interfaces;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Events;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Application.EventHandlers;

/// <summary>
/// 合同签署后触发：
/// 1. 将 ProjectSlot 状态流转为 Occupied，绑定 InvestorUserId
/// 2. 生成合同 PDF，更新 PdfStoragePath → 状态流转到 Executed
/// （Epic 11 可扩展：推站内通知）
/// </summary>
public class ContractSignedEventHandler(
    IContractRepository contractRepository,
    IProjectSlotRepository slotRepository,
    IPdfGeneratorService pdfGenerator) : INotificationHandler<ContractSignedEvent>
{
    public async Task Handle(ContractSignedEvent evt, CancellationToken ct)
    {
        // ── 1. 槽位状态流转 ────────────────────────────────────────────
        try
        {
            var slot = await slotRepository.GetByIdAsync(evt.SlotId, ct);
            if (slot is not null && slot.Status != SlotStatus.Occupied)
            {
                // Reserved → Occupied（若尚未 Reserve 则直接 Reserve+Confirm）
                if (slot.Status == SlotStatus.Available)
                    slot.Reserve(evt.InvestorUserId);

                slot.Confirm();   // Status = Occupied
                await slotRepository.UpdateAsync(slot, ct);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ContractSignedEventHandler] Slot update failed for slot {evt.SlotId}: {ex.Message}");
        }

        // ── 2. 生成合同 PDF ───────────────────────────────────────────
        try
        {
            var pdfBytes = await pdfGenerator.GenerateContractPdfAsync(evt.ContractId, ct);

            var storagePath = $"contracts/{evt.ContractId}.pdf";
            var dir = Path.GetDirectoryName(storagePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            await File.WriteAllBytesAsync(storagePath, pdfBytes, ct);

            var contract = await contractRepository.GetByIdAsync(evt.ContractId, ct);
            if (contract is not null)
            {
                contract.PdfGenerated(storagePath);
                await contractRepository.UpdateAsync(contract, ct);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ContractSignedEventHandler] PDF generation failed for {evt.ContractId}: {ex.Message}");
        }
    }
}
