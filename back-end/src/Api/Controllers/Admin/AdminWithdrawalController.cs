using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareFlow.Api.Filters;
using ShareFlow.Application.Common;
using ShareFlow.Application.Wallet.DTOs;
using ShareFlow.Application.Wallet.Interfaces;
using System.Security.Claims;

namespace ShareFlow.Api.Controllers.Admin;

[ApiController]
[Route("v1/admin/withdrawals")]
[Authorize]
public class AdminWithdrawalController(IWalletService walletService) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>提现申请列表</summary>
    [HttpGet]
    [Permission("dividend.read")]
    public async Task<IActionResult> GetListAsync(
        [FromQuery] string? status, [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await walletService.GetWithdrawalRequestsAsync(status, page, pageSize, keyword, ct);
        return Ok(ApiResponse<PagedResult<WithdrawalRequestDto>>.Success(result));
    }

    /// <summary>批准提现申请</summary>
    [HttpPost("{id:guid}/approve")]
    [Permission("withdrawal.approve")]
    public async Task<IActionResult> ApproveAsync(Guid id, CancellationToken ct)
    {
        await walletService.ApproveWithdrawalAsync(id, CurrentUserId, ct);
        return Ok(ApiResponse<object>.Success(new { }));
    }

    /// <summary>标记提现已完成（线下打款完毕）</summary>
    [HttpPost("{id:guid}/complete")]
    [Permission("withdrawal.approve")]
    public async Task<IActionResult> CompleteAsync(Guid id, CancellationToken ct)
    {
        await walletService.CompleteWithdrawalAsync(id, CurrentUserId, ct);
        return Ok(ApiResponse<object>.Success(new { }));
    }

    /// <summary>拒绝提现申请</summary>
    [HttpPost("{id:guid}/reject")]
    [Permission("withdrawal.approve")]
    public async Task<IActionResult> RejectAsync(Guid id, [FromBody] RejectWithdrawalRequest request, CancellationToken ct)
    {
        await walletService.RejectWithdrawalAsync(id, CurrentUserId, request.Reason, ct);
        return Ok(ApiResponse<object>.Success(new { }));
    }
}
