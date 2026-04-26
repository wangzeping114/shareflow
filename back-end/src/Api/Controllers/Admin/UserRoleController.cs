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
[Route("v1/admin/users")]
[Permission(Permissions.UserManage)]
public class UserRoleController(IRoleService roleService) : ControllerBase
{
    [HttpPost("{id:guid}/role")]
    public async Task<IActionResult> AssignRoleAsync(Guid id, [FromBody] AssignBackendRoleRequest request, CancellationToken cancellationToken)
    {
        await roleService.AssignRoleToUserAsync(id, request, cancellationToken);
        return Ok(ApiResponse<object>.Success(new { }));
    }
}