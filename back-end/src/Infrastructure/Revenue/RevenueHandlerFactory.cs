using ShareFlow.Application.Revenue.Interfaces;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Infrastructure.Revenue;

public class RevenueHandlerFactory(IRegionContext regionContext) : IPlatformRevenueHandlerFactory
{
    private readonly Dictionary<string, IPlatformRevenueHandler> _csvHandlers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tiktok"] = new TikTokCsvHandler(regionContext),
        ["youtube"] = new YouTubeCsvHandler(regionContext),
        ["kwai"] = new TikTokCsvHandler(regionContext),     // 复用 TikTok 格式
        ["instagram"] = new TikTokCsvHandler(regionContext), // 复用 TikTok 格式
        ["douyin"] = new TikTokCsvHandler(regionContext),
        ["kuaishou"] = new TikTokCsvHandler(regionContext),
        ["xiaohongshu"] = new TikTokCsvHandler(regionContext),
    };

    public IPlatformRevenueHandler GetHandler(string platformName, RevenueImportSource source)
    {
        if (source == RevenueImportSource.Csv)
        {
            if (_csvHandlers.TryGetValue(platformName, out var handler))
                return handler;
            // 降级到通用 TikTok 格式解析器
            return new TikTokCsvHandler(regionContext);
        }

        throw new NotSupportedException($"Unsupported source: {source} for platform: {platformName}");
    }
}
