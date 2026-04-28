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