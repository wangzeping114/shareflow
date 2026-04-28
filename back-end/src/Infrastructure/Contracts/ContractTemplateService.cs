using ShareFlow.Application.Contracts.Interfaces;

namespace ShareFlow.Infrastructure.Contracts;

/// <summary>合同模板服务 — 内置中英文标准化模板，按 TemplateType 渲染</summary>
public class ContractTemplateService : IContractTemplateService
{
    // ──────────────────────────────────────────────────────────────
    //  英文模板（OverseasEnglish）— 默认海外实例使用
    // ──────────────────────────────────────────────────────────────
    private const string EnglishTemplate = """
        SHARE DIVIDEND AGREEMENT

        This Share Dividend Agreement ("Agreement") is entered into as of {EffectiveDate}, by and between:

        Investor Name : {InvestorName}
        Project Title : {ProjectTitle}
        Platform(s)   : {PlatformName}
        Share Permille: {SharePermille}‰  (per mille of total platform revenue)
        Settlement    : {Currency}

        1. PURPOSE
        The Investor holds a proportional share interest in the above-named short-video project operated
        on {PlatformName}. The Investor is entitled to receive dividend distributions calculated on the
        basis of {SharePermille}‰ of the net platform revenue attributable to the project.

        2. DIVIDEND DISTRIBUTION
        Dividends shall be calculated and distributed in accordance with the platform's official revenue
        statements. ShareFlow shall initiate transfers to the Investor's designated wallet within five (5)
        business days following each settlement cycle.

        3. TERM
        This Agreement takes effect on {EffectiveDate} and remains in force until the Investor's share is
        fully redeemed, transferred, or superseded by a renewed agreement.

        4. REPRESENTATIONS
        The Investor confirms that all information provided is accurate and that they accept the terms of
        this Agreement by affixing their electronic signature below.

        5. GOVERNING LAW
        This Agreement shall be governed by and construed in accordance with applicable international
        commercial law, as agreed between the parties.

        ──────────────────────────────────────────────
        Investor Signature: ___________________________
        Date Signed       : ___________________________
        ──────────────────────────────────────────────
        """;

    // ──────────────────────────────────────────────────────────────
    //  中文模板（DomesticChinese）
    // ──────────────────────────────────────────────────────────────
    private const string ChineseTemplate = """
        短视频持股分红协议

        本协议（以下简称"协议"）于 {EffectiveDate} 由以下双方签订：

        投资人姓名：{InvestorName}
        项目名称  ：{ProjectTitle}
        运营平台  ：{PlatformName}
        持股千分比：{SharePermille}‰（占平台收益总额的比例）
        结算货币  ：{Currency}

        一、协议目的
        投资人持有上述短视频项目的比例权益，有权按照 {SharePermille}‰ 的比例享受
        {PlatformName} 平台产生的净收益分红。

        二、分红分配
        分红金额依据平台官方收益报告计算，ShareFlow 将在每个结算周期结束后五（5）个工作日内
        将款项转入投资人指定钱包。

        三、协议期限
        本协议自 {EffectiveDate} 起生效，持续有效至投资人持股被完全赎回、转让或被续签协议
        所取代为止。

        四、声明
        投资人确认所提供信息准确无误，并通过在下方签署电子签名，表示接受本协议全部条款。

        五、适用法律
        本协议适用中华人民共和国相关法律法规进行解释和执行。

        ──────────────────────────────────────────────
        投资人签名：___________________________
        签署日期  ：___________________________
        ──────────────────────────────────────────────
        """;

    private static readonly Dictionary<string, string> Templates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["OverseasEnglish"] = EnglishTemplate,
        ["DomesticChinese"] = ChineseTemplate,
    };

    public string Render(string templateType, ContractTemplateData data)
    {
        var raw = GetRaw(templateType);
        return raw
            .Replace("{ProjectTitle}", data.ProjectTitle)
            .Replace("{PlatformName}", data.PlatformName)
            .Replace("{InvestorName}", data.InvestorName)
            .Replace("{SharePermille}", data.SharePermille.ToString("F4"))
            .Replace("{Currency}", data.Currency)
            .Replace("{TotalInvestment}", data.TotalInvestment.ToString("N2"))
            .Replace("{EffectiveDate}", data.EffectiveDate);
    }

    public string GetRaw(string templateType)
    {
        if (Templates.TryGetValue(templateType, out var template))
            return template;

        // 未知模板类型降级到英文
        return EnglishTemplate;
    }
}
