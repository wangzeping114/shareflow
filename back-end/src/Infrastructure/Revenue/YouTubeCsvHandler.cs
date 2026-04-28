using ShareFlow.Application.Revenue.DTOs;
using ShareFlow.Application.Revenue.Interfaces;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Infrastructure.Revenue;

/// <summary>
/// YouTube Analytics CSV 格式解析器
/// 预期列头：Day, Estimated revenue (USD)（或 Date, Revenue, Currency）
/// </summary>
public class YouTubeCsvHandler(IRegionContext regionContext) : IPlatformRevenueHandler
{
    public RevenueImportSource SupportedSource => RevenueImportSource.Csv;

    public Task<IReadOnlyList<ParsedRevenueItem>> ParseAsync(RevenueParseRequest request, CancellationToken ct = default)
    {
        if (request.CsvStream is null)
            throw new ArgumentException("CSV stream is required.");

        var results = new List<ParsedRevenueItem>();
        using var reader = new System.IO.StreamReader(request.CsvStream);

        // 跳过 YouTube Analytics 导出文件的前几行元数据
        string? line;
        string[]? header = null;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith("Day,") || line.StartsWith("Date,") || line.StartsWith("date,"))
            {
                header = line.Split(',');
                break;
            }
        }

        if (header is null) return Task.FromResult<IReadOnlyList<ParsedRevenueItem>>(results);

        var dateIdx = FindIndex(header, "day", "date");
        var amountIdx = FindIndex(header, "estimated revenue (usd)", "revenue", "amount", "estimated_revenue");
        var currencyIdx = FindIndex(header, "currency");

        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Total")) continue;
            var cols = line.Split(',');

            if (!DateTime.TryParse(SafeGet(cols, dateIdx), out var date)) continue;
            if (!decimal.TryParse(SafeGet(cols, amountIdx), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var amount)) continue;

            var currency = currencyIdx >= 0
                ? SafeGet(cols, currencyIdx).Trim()
                : regionContext.DefaultCurrency;

            results.Add(new ParsedRevenueItem { Amount = amount, Currency = currency, RevenueDate = date });
        }

        return Task.FromResult<IReadOnlyList<ParsedRevenueItem>>(results);
    }

    private static int FindIndex(string[] header, params string[] names)
    {
        for (var i = 0; i < header.Length; i++)
            if (names.Any(n => header[i].Trim().Trim('"').Equals(n, StringComparison.OrdinalIgnoreCase)))
                return i;
        return -1;
    }

    private static string SafeGet(string[] cols, int idx) =>
        idx >= 0 && idx < cols.Length ? cols[idx].Trim().Trim('"') : string.Empty;
}
