using Microsoft.AspNetCore.Mvc;

namespace ShareFlow.Api.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class PermissionAttribute(string permission) : TypeFilterAttribute(typeof(PermissionFilter))
{
    public string Permission { get; } = permission;
}