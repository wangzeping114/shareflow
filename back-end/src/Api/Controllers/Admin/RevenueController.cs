using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareFlow.Api.Filters;
using ShareFlow.Application.Common;
using ShareFlow.Application.Revenue.DTOs;
using ShareFlow.Application.Revenue.Interfaces;
using System.Security.Claims;

namespace ShareFlow.Api.Controllers.Admin;

[ApiController]
[Route("v1/admin/revenues")]
[Authorize]
public class RevenueController(IRevenueService revenueService) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [Permission("revenue.read")]
    public async Task<IActionResult> GetListAsync([FromQuery] RevenueQueryRequest query, CancellationToken ct)
    {
        var result = await revenueService.GetListAsync(query, ct);
        return Ok(ApiResponse<PagedResult<RevenueDto>>.Success(result));
    }

    [HttpPost("manual")]
    [Permission("revenue.write")]
    public async Task<IActionResult> AddManualAsync([FromBody] AddManualRevenueRequest request, CancellationToken ct)
    {
        var id = await revenueService.AddManualAsync(request, CurrentUserId, ct);
        return Ok(ApiResponse<object>.Success(new { id }));
    }

    [HttpPost("csv")]
    [Permission("revenue.write")]
    public async Task<IActionResult> ImportCsvAsync(
        [FromQuery] Guid projectId,
        [FromQuery] string platform,
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("请上传 CSV 文件。"));

        await using var stream = file.OpenReadStream();
        var result = await revenueService.ImportCsvAsync(projectId, platform, stream, ct);
        return Ok(ApiResponse<BatchImportResult>.Success(result));
    }

    [HttpPost("screenshot-preview")]
    [Permission("revenue.write")]
    public async Task<IActionResult> PreviewScreenshotAsync(
        [FromQuery] Guid projectId,
        [FromQuery] string platform,
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("请上传截图文件。"));

        byte[] fileBytes;
        await using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms, ct);
            fileBytes = ms.ToArray();
        }

        var imageBase64 = Convert.ToBase64String(fileBytes);
        var mimeType = file.ContentType;

        var result = await revenueService.PreviewScreenshotAsync(projectId, platform, imageBase64, mimeType, ct);
        return Ok(ApiResponse<AiImportPreviewDto>.Success(result));
    }

    [HttpPost("{id:guid}/confirm-ai")]
    [Permission("revenue.write")]
    public async Task<IActionResult> ConfirmAiImportAsync(
        Guid id, [FromBody] ConfirmAiImportRequest request, CancellationToken ct)
    {
        await revenueService.ConfirmAiImportAsync(id, request.OverrideAmount, ct);
        return Ok(ApiResponse<object>.Success(new { }));
    }

    [HttpPost("{id:guid}/approve")]
    [Permission("revenue.verify")]
    public async Task<IActionResult> ApproveAsync(Guid id, CancellationToken ct)
    {
        await revenueService.ApproveAsync(id, CurrentUserId, ct);
        return Ok(ApiResponse<object>.Success(new { }));
    }

    [HttpPost("{id:guid}/reject")]
    [Permission("revenue.verify")]
    public async Task<IActionResult> RejectAsync(
        Guid id, [FromBody] RejectRevenueRequest request, CancellationToken ct)
    {
        await revenueService.RejectAsync(id, CurrentUserId, request.Reason, ct);
        return Ok(ApiResponse<object>.Success(new { }));
    }
}
