namespace ShareFlow.Domain.Interfaces;

public enum RegionMode { Overseas, Domestic }

public interface IRegionContext
{
    RegionMode Mode { get; }
    string DefaultCurrency { get; }
    string DefaultLocale { get; }
    IReadOnlyList<string> EnabledPlatforms { get; }
    bool IsYouTubeOAuthEnabled { get; }
}
