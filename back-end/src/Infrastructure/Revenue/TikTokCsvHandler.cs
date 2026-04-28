using ShareFlow.Application.Revenue.DTOs;
using ShareFlow.Application.Revenue.Interfaces;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Infrastructure.Revenue;

/// <summary>
/// TikTok Creator Marketplace CSV 格式解析器
/// 预期列头：Date, Revenue, Currency（顺序不限，大小写不敏感）
/// </summary>
public class TikTokCsvHandler(IRegionContext regionContext) : IPlatformRevenueHandler
{
    public RevenueImportSource SupportedSource => RevenueImportSource.Csv;

    public Task<IReadOnlyList<ParsedRevenueItem>> ParseAsync(RevenueParseRequest request, CancellationToken ct = default)
    {
        if (request.CsvStream is null)
            throw new ArgumentException("CSV stream is required.");

        var results = new List<ParsedRevenueItem>();
        using var reader = new System.IO.StreamReader(request.CsvStream);

        var header = reader.ReadLine()?.Split(',');
        if (header is null) return Task.FromResult<IReadOnlyList<ParsedRevenueItem>>(results);

        var dateIdx = FindIndex(header, "date");
        var amountIdx = FindIndex(header, "revenue", "amount");
        var currencyIdx = FindIndex(header, "currency");

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;
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
            if (names.Any(n => header[i].Trim().Equals(n, StringComparison.OrdinalIgnoreCase)))
                return i;
        return -1;
    }

    private static string SafeGet(string[] cols, int idx) =>
        idx >= 0 && idx < cols.Length ? cols[idx].Trim().Trim('"') : string.Empty;
}
