using Microsoft.AspNetCore.Mvc;
using ShareFlow.Api.Filters;
using ShareFlow.Application.Admin.DTOs;
using ShareFlow.Application.Admin.Interfaces;
using ShareFlow.Application.Common;
using ShareFlow.Domain.Constants;

namespace ShareFlow.Api.Controllers.Admin;

[ApiController]
[Route("v1/admin/dashboard")]
public class AdminDashboardController(IAdminDashboardService dashboardService) : ControllerBase
{
    /// <summary>管理端总览数据（KPI + 图表数据）</summary>
    [HttpGet]
    [Permission(Permissions.DashboardRead)]
    public async Task<IActionResult> GetAsync(CancellationToken ct)
    {
        var result = await dashboardService.GetDashboardAsync(ct);
        return Ok(ApiResponse<AdminDashboardDto>.Success(result));
    }
}
