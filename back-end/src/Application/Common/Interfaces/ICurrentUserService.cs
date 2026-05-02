namespace ShareFlow.Application.Common.Interfaces;

/// <summary>从 HTTP 上下文提取当前登录用户信息，注入 Application 层服务。</summary>
public interface ICurrentUserService
{
    Guid UserId { get; }
    string Role { get; }
}
