using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareFlow.Api.Filters;
using ShareFlow.Application.Common;
using ShareFlow.Application.Wallet.DTOs;
using ShareFlow.Application.Wallet.Interfaces;
using System.Security.Claims;

namespace ShareFlow.Api.Controllers.Admin;

[ApiController]
[Route("v1/admin/wallets")]
[Authorize]
public class AdminWalletController(IWalletService walletService) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>所有投资人钱包余额列表</summary>
    [HttpGet]
    [Permission("dividend.read")]
    public async Task<IActionResult> GetListAsync([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await walletService.GetAllAsync(page, pageSize, keyword, ct);
        return Ok(ApiResponse<PagedResult<WalletDto>>.Success(result));
    }

    /// <summary>查看指定投资人的钱包</summary>
    [HttpGet("{investorId:guid}")]
    [Permission("dividend.read")]
    public async Task<IActionResult> GetByInvestorAsync(Guid investorId, CancellationToken ct)
    {
        var result = await walletService.GetByInvestorIdAsync(investorId, ct);
        return Ok(ApiResponse<WalletDto>.Success(result));
    }

    /// <summary>查看指定投资人的流水记录</summary>
    [HttpGet("{investorId:guid}/transactions")]
    [Permission("dividend.read")]
    public async Task<IActionResult> GetTransactionsAsync(Guid investorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await walletService.GetTransactionsAsync(investorId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<WalletTransactionDto>>.Success(result));
    }

    /// <summary>管理员为投资人充值</summary>
    [HttpPost("{investorId:guid}/credit")]
    [Permission("dividend.write")]
    public async Task<IActionResult> CreditAsync(Guid investorId, [FromBody] AdminCreditRequest request, CancellationToken ct)
    {
        await walletService.AdminCreditAsync(investorId, request.Amount, request.Remark, CurrentUserId, ct);
        return Ok(ApiResponse<object>.Success(new { }));
    }
}
