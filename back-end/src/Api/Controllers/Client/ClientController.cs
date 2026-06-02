using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareFlow.Api.Filters;
using ShareFlow.Application.Client.Interfaces;
using ShareFlow.Application.Common;
using System.Security.Claims;

namespace ShareFlow.Api.Controllers.Client;

[ApiController]
[Route("v1/client")]
[Authorize]
public class ClientController(IClientDashboardService dashboardService) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>客户 Dashboard 汇总数据</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardAsync(CancellationToken ct)
    {
        var result = await dashboardService.GetDashboardAsync(CurrentUserId, ct);
        return Ok(ApiResponse<object>.Success(result));
    }

    /// <summary>客户分红记录（分页）</summary>
    [HttpGet("dividends")]
    public async Task<IActionResult> GetDividendsAsync(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await dashboardService.GetDividendsAsync(CurrentUserId, page, pageSize, ct);
        return Ok(ApiResponse<object>.Success(result));
    }

    /// <summary>客户合同列表</summary>
    [HttpGet("contracts")]
    public async Task<IActionResult> GetContractsAsync(CancellationToken ct)
    {
        var result = await dashboardService.GetContractsAsync(CurrentUserId, ct);
        return Ok(ApiResponse<object>.Success(result));
    }

    /// <summary>获取合同 PDF 下载路径（仅本人合同）</summary>
    [HttpGet("contracts/{id:guid}/pdf-path")]
    public async Task<IActionResult> GetContractPdfPathAsync(Guid id, CancellationToken ct)
    {
        var path = await dashboardService.GetContractPdfPathAsync(id, CurrentUserId, ct);
        return Ok(ApiResponse<object>.Success(new { path }));
    }

    /// <summary>预览合同 PDF（仅本人合同）</summary>
    [HttpGet("contracts/{id:guid}/pdf-preview")]
    public async Task<IActionResult> PreviewContractPdfAsync(Guid id, CancellationToken ct)
    {
        var content = await dashboardService.GetContractPdfContentAsync(id, CurrentUserId, ct);
        if (content is null)
        {
            return NotFound(ApiResponse<object>.Fail("pdf.notFound", 404));
        }

        Response.Headers.ContentDisposition = $"inline; filename=contract-{id}.pdf";
        return File(content, "application/pdf");
    }
}
