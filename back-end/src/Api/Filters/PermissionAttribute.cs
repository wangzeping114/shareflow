using Microsoft.AspNetCore.Mvc;

namespace ShareFlow.Api.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class PermissionAttribute : TypeFilterAttribute
{
    public string Permission { get; }

    public PermissionAttribute(string permission) : base(typeof(PermissionFilter))
    {
        Permission = permission;
        Arguments = [permission];   // 把 string 参数注入给 PermissionFilter 构造函数
    }
}