using ShareFlow.Application.Common;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Tests;

public class ClientTextLocalizerTests
{
    [Fact]
    public void UseEnglish_EnUsLocale_ReturnsTrue()
    {
        var regionContext = new FakeRegionContext("en-US");

        Assert.True(ClientTextLocalizer.UseEnglish(regionContext));
    }

    [Theory]
    [InlineData(DividendStatus.Distributed, true, "Credited")]
    [InlineData(DividendStatus.Distributed, false, "已到账")]
    public void GetDividendStatusLabel_ReturnsExpectedText(DividendStatus status, bool useEnglish, string expected)
    {
        var actual = ClientTextLocalizer.GetDividendStatusLabel(status, useEnglish);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(WithdrawalStatus.Completed, true, "Completed")]
    [InlineData(WithdrawalStatus.Completed, false, "已完成")]
    public void GetWithdrawalStatusLabel_ReturnsExpectedText(WithdrawalStatus status, bool useEnglish, string expected)
    {
        var actual = ClientTextLocalizer.GetWithdrawalStatusLabel(status, useEnglish);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("分红到账", true, "Dividend credited")]
    [InlineData("提现已完成", true, "Withdrawal completed")]
    [InlineData("Dividend credited", false, "分红到账")]
    public void LocalizeWalletRemark_ReturnsExpectedText(string input, bool useEnglish, string expected)
    {
        var actual = ClientTextLocalizer.LocalizeWalletRemark(input, useEnglish);

        Assert.Equal(expected, actual);
    }

    private sealed class FakeRegionContext(string defaultLocale) : IRegionContext
    {
        public RegionMode Mode => RegionMode.Overseas;
        public string DefaultCurrency => "USD";
        public string DefaultLocale => defaultLocale;
        public IReadOnlyList<string> EnabledPlatforms => [];
        public bool IsYouTubeOAuthEnabled => true;
    }
}
