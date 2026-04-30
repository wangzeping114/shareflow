using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareFlow.Application.Common;
using ShareFlow.Application.Wallet.DTOs;
using ShareFlow.Application.Wallet.Interfaces;
using System.Security.Claims;

namespace ShareFlow.Api.Controllers.Client;

[ApiController]
[Route("v1/client/wallet")]
[Authorize]
public class ClientWalletController(IWalletService walletService) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>查看自己的钱包余额</summary>
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken ct)
    {
        var result = await walletService.GetByInvestorIdAsync(CurrentUserId, ct);
        return Ok(ApiResponse<WalletDto>.Success(result));
    }

    /// <summary>查看自己的流水记录</summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactionsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await walletService.GetTransactionsAsync(CurrentUserId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<WalletTransactionDto>>.Success(result));
    }

    /// <summary>申请提现</summary>
    [HttpPost("withdrawals")]
    public async Task<IActionResult> RequestWithdrawalAsync([FromBody] RequestWithdrawalRequest request, CancellationToken ct)
    {
        var id = await walletService.RequestWithdrawalAsync(CurrentUserId, request.Amount, request.PaymentMethod, ct);
        return Ok(ApiResponse<object>.Success(new { id }));
    }

    /// <summary>查看自己的提现申请记录</summary>
    [HttpGet("withdrawals")]
    public async Task<IActionResult> GetWithdrawalsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await walletService.GetMyWithdrawalRequestsAsync(CurrentUserId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<WithdrawalRequestDto>>.Success(result));
    }
}
