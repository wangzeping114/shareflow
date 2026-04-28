namespace ShareFlow.Application.Contracts.Interfaces;

/// <summary>合同模板渲染数据</summary>
public record ContractTemplateData(
    string ProjectTitle,
    string PlatformName,
    string InvestorName,
    decimal SharePermille,
    string Currency,
    decimal TotalInvestment,
    string EffectiveDate);

/// <summary>合同模板服务：按 TemplateType 渲染标准化合同正文</summary>
public interface IContractTemplateService
{
    /// <summary>根据模板类型渲染合同文本，返回完整合同内容字符串</summary>
    string Render(string templateType, ContractTemplateData data);

    /// <summary>获取指定类型的模板原文（不含数据替换）</summary>
    string GetRaw(string templateType);
}
