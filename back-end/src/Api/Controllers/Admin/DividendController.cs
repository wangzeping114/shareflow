using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareFlow.Api.Filters;
using ShareFlow.Application.Common;
using ShareFlow.Application.Dividend.DTOs;
using ShareFlow.Application.Dividend.Interfaces;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;
using System.Security.Claims;

namespace ShareFlow.Api.Controllers.Admin;

[ApiController]
[Route("v1/admin/dividends")]
[Authorize]
public class DividendController(
    IDividendService dividendService,
    IDividendCalculationService calcService,
    IContractRepository contractRepository,
    IProjectSlotRepository slotRepository) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [Permission("dividend.read")]
    public async Task<IActionResult> GetListAsync([FromQuery] DividendQueryRequest query, CancellationToken ct)
    {
        var result = await dividendService.GetListAsync(query, ct);
        return Ok(ApiResponse<PagedResult<DividendDto>>.Success(result));
    }

    [HttpGet("summary")]
    [Permission("dividend.read")]
    public async Task<IActionResult> GetSummaryAsync([FromQuery] Guid projectId, CancellationToken ct)
    {
        var result = await dividendService.GetProjectSummaryAsync(projectId, ct);
        return Ok(ApiResponse<DividendProjectSummaryDto>.Success(result));
    }

    [HttpPost("confirm")]
    [Permission("dividend.write")]
    public async Task<IActionResult> ConfirmBatchAsync([FromBody] BatchDividendRequest request, CancellationToken ct)
    {
        await calcService.ConfirmBatchAsync(request.DividendIds, CurrentUserId, ct);
        return Ok(ApiResponse<object>.Success(new { }));
    }

    [HttpPost("distribute")]
    [Permission("dividend.write")]
    public async Task<IActionResult> DistributeBatchAsync([FromBody] BatchDividendRequest request, CancellationToken ct)
    {
        await calcService.DistributeBatchAsync(request.DividendIds, CurrentUserId, ct);
        return Ok(ApiResponse<object>.Success(new { }));
    }

    /// <summary>
    /// 手动触发对某条已审核收益的分红补算（处理存量数据 / 事件处理器异常导致未计算的情况）
    /// </summary>
    [HttpPost("{revenueId:guid}/recalculate")]
    [Permission("dividend.write")]
    public async Task<IActionResult> RecalculateAsync(Guid revenueId, CancellationToken ct)
    {
        await calcService.CalculateForRevenueAsync(revenueId, ct);
        return Ok(ApiResponse<object>.Success(new { }));
    }

    /// <summary>
    /// 修复历史数据：将所有已签署合同关联的槽位强制设置为 Occupied 状态。
    /// 用于处理早期 ContractSignedEventHandler 未正确更新槽位的存量数据。
    /// </summary>
    [HttpPost("repair-slots")]
    [Permission("dividend.write")]
    public async Task<IActionResult> RepairSlotsAsync(CancellationToken ct)
    {
        var signedContracts = await contractRepository.GetSignedAsync(ct);
        var repairedCount = 0;

        foreach (var contract in signedContracts)
        {
            var slot = await slotRepository.GetByIdAsync(contract.SlotId, ct);
            if (slot is null || slot.Status == SlotStatus.Occupied)
                continue;

            if (slot.Status == SlotStatus.Available)
                slot.Reserve(contract.InvestorUserId);

            if (slot.Status == SlotStatus.Reserved)
            {
                slot.Confirm();
                await slotRepository.UpdateAsync(slot, ct);
                repairedCount++;
            }
        }

        return Ok(ApiResponse<object>.Success(new { repairedCount, message = $"已修复 {repairedCount} 个槽位状态为 Occupied" }));
    }
}
