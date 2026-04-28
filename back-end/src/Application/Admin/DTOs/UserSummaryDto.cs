namespace ShareFlow.Application.Admin.DTOs;

/// <summary>投资人/用户简要信息（用于前端下拉选择器）</summary>
public record UserSummaryDto(Guid Id, string DisplayName, string Email);
