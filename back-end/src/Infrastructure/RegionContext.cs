using Microsoft.Extensions.Configuration;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Infrastructure;

public class RegionContext : IRegionContext
{
    public RegionMode Mode { get; }
    public string DefaultCurrency { get; }
    public string DefaultLocale { get; }
    public IReadOnlyList<string> EnabledPlatforms { get; }
    public bool IsYouTubeOAuthEnabled { get; }

    public RegionContext(IConfiguration config)
    {
        var regionStr = config["REGION"] ?? "overseas";
        Mode = regionStr == "domestic" ? RegionMode.Domestic : RegionMode.Overseas;
        DefaultCurrency = config["DEFAULT_CURRENCY"] ?? "USD";
        DefaultLocale = config["CLIENT_LOCALE"] ?? "en-US";
        var platforms = config["ENABLED_PLATFORMS"] ?? "YouTube,TikTok,Instagram,BrandDeal";
        EnabledPlatforms = platforms.Split(',', StringSplitOptions.RemoveEmptyEntries);
        IsYouTubeOAuthEnabled = bool.TryParse(config["YOUTUBE_OAUTH_ENABLED"], out var v) && v;
    }
}
