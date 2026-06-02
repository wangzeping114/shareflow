using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ShareFlow.Application.Contracts.Interfaces;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Infrastructure.ESign;

/// <summary>使用 QuestPDF 生成合同 PDF，包含合同内容快照和客户签名图</summary>
public class PdfGeneratorService(IContractRepository contractRepo) : IPdfGeneratorService
{
    static PdfGeneratorService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateContractPdfAsync(Guid contractId, CancellationToken ct = default)
    {
        var contract = await contractRepo.GetByIdWithDetailsAsync(contractId, ct)
            ?? throw new InvalidOperationException($"Contract {contractId} not found");

        var investorName = contract.InvestorUser?.DisplayName ?? "—";
        var projectTitle = contract.Project?.Title ?? "—";
        var snapshot     = contract.ContractSnapshot ?? string.Empty;
        var signedAt     = contract.SignedAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "—";

        // 解析签名图 base64
        byte[]? sigImageBytes = null;
        if (!string.IsNullOrEmpty(contract.SignatureDataUrl))
        {
            try
            {
                var commaIdx = contract.SignatureDataUrl.IndexOf(',');
                var base64   = commaIdx >= 0 ? contract.SignatureDataUrl[(commaIdx + 1)..] : contract.SignatureDataUrl;
                sigImageBytes = Convert.FromBase64String(base64);
            }
            catch { /* 忽略解析失败 */ }
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x
                    .FontFamily("Noto Sans CJK SC", "Noto Sans CJK TC", "Noto Sans CJK JP", "Noto Sans CJK KR", "Arial Unicode MS", "Microsoft YaHei", "SimSun")
                    .FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("ShareFlow — 持股分红合同")
                        .FontSize(18).Bold().AlignCenter();
                    col.Item().PaddingTop(4)
                        .Text($"合同编号：{contractId}")
                        .FontSize(8).FontColor(Colors.Grey.Medium).AlignCenter();
                    col.Item().PaddingVertical(8).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().Column(col =>
                {
                    // 基本信息
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(5); });
                        void AddRow(string label, string value)
                        {
                            table.Cell().PaddingVertical(4).Text(label).Bold();
                            table.Cell().PaddingVertical(4).Text(value);
                        }
                        AddRow("项目名称", projectTitle);
                        AddRow("持股人", investorName);
                        AddRow("持股千分比", contract.Slot != null
                            ? $"{contract.Slot.SharePermille:F4}‰  ({contract.Slot.SharePermille / 10:F2}%)"
                            : "—");
                        AddRow("签署时间", signedAt);
                    });

                    col.Item().PaddingVertical(12).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                    // 合同内容快照
                    if (!string.IsNullOrWhiteSpace(snapshot))
                    {
                        col.Item().PaddingBottom(8).Text("合同内容").FontSize(12).Bold();
                        col.Item().Background(Colors.Grey.Lighten5).Padding(10)
                            .Text(snapshot).FontSize(9).LineHeight(1.5f);
                    }

                    col.Item().PaddingVertical(12).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                    // 签名区域
                    col.Item().Text("持股人电子签名").FontSize(11).Bold();
                    col.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("签署时间").FontSize(9).FontColor(Colors.Grey.Medium);
                            c.Item().PaddingTop(2).Text(signedAt).FontSize(10);
                        });

                        if (sigImageBytes is not null)
                        {
                            row.ConstantItem(200).Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(4).Image(sigImageBytes).FitArea();
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("ShareFlow — ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });

        return document.GeneratePdf();
    }
}
