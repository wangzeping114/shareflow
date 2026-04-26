using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareFlow.Api.Filters;
using ShareFlow.Application.Admin.DTOs;
using ShareFlow.Application.Admin.Interfaces;
using ShareFlow.Application.Common;
using ShareFlow.Domain.Constants;

namespace ShareFlow.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("v1/admin/roles")]
[Permission(Permissions.RoleManage)]
public class RoleController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var roles = await roleService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BackendRoleDto>>.Success(roles));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateBackendRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await roleService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<BackendRoleDto>.Success(role));
    }

    [HttpPut("{id:guid}/permissions")]
    public async Task<IActionResult> UpdatePermissionsAsync(Guid id, [FromBody] UpdateBackendRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        var role = await roleService.UpdatePermissionsAsync(id, request, cancellationToken);
        return Ok(ApiResponse<BackendRoleDto>.Success(role));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await roleService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Success(new { }));
    }
}