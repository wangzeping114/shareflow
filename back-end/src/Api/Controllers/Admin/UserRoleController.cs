using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareFlow.Api.Filters;
using ShareFlow.Application.Admin.DTOs;
using ShareFlow.Application.Admin.Interfaces;
using ShareFlow.Application.Common;
using ShareFlow.Domain.Constants;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("v1/admin/users")]
[Permission(Permissions.UserManage)]
public class UserRoleController(IRoleService roleService, IUserRepository userRepository) : ControllerBase
{
    /// <summary>获取所有内部账号（BackendCustom + Sales）</summary>
    [HttpGet("internal")]
    public async Task<IActionResult> GetInternalUsersAsync(CancellationToken ct)
    {
        var users = await roleService.GetInternalUsersAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<InternalUserDto>>.Success(users));
    }

    /// <summary>创建内部账号</summary>
    [HttpPost("internal")]
    public async Task<IActionResult> CreateInternalUserAsync([FromBody] CreateInternalUserRequest request, CancellationToken ct)
    {
        var user = await roleService.CreateInternalUserAsync(request, ct);
        return Ok(ApiResponse<InternalUserDto>.Success(user));
    }

    /// <summary>获取所有 Client 角色用户（合同投资人选择器用）</summary>
    [HttpGet("clients")]
    public async Task<IActionResult> GetClientsAsync(CancellationToken ct)
    {
        var users = await userRepository.GetByRoleAsync(UserRole.Client, ct);
        var result = users.Select(u => new UserSummaryDto(u.Id, u.DisplayName, u.Email)).ToList();
        return Ok(ApiResponse<List<UserSummaryDto>>.Success(result));
    }

    [HttpPost("{id:guid}/role")]
    public async Task<IActionResult> AssignRoleAsync(Guid id, [FromBody] AssignBackendRoleRequest request, CancellationToken cancellationToken)
    {
        await roleService.AssignRoleToUserAsync(id, request, cancellationToken);
        return Ok(ApiResponse<object>.Success(new { }));
    }
}