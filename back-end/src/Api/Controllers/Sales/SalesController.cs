using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareFlow.Application.Common;
using ShareFlow.Application.Sales.DTOs;
using ShareFlow.Application.Sales.Interfaces;
using System.Security.Claims;

namespace ShareFlow.Api.Controllers.Sales;

[ApiController]
[Route("v1/sales")]
[Authorize]
public class SalesController(ISalesService salesService) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("leads")]
    public async Task<IActionResult> GetLeadsAsync([FromQuery] LeadQueryRequest query, CancellationToken ct)
    {
        var result = await salesService.GetLeadsAsync(CurrentUserId, query, ct);
        return Ok(ApiResponse<PagedResult<LeadDto>>.Success(result));
    }

    [HttpPost("leads")]
    public async Task<IActionResult> CreateLeadAsync([FromBody] CreateLeadRequest request, CancellationToken ct)
    {
        var result = await salesService.CreateLeadAsync(CurrentUserId, request, ct);
        return Ok(ApiResponse<LeadDto>.Success(result));
    }

    [HttpPut("leads/{id:guid}")]
    public async Task<IActionResult> UpdateLeadAsync(Guid id, [FromBody] UpdateLeadRequest request, CancellationToken ct)
    {
        var result = await salesService.UpdateLeadAsync(CurrentUserId, id, request, ct);
        return Ok(ApiResponse<LeadDto>.Success(result));
    }

    [HttpGet("projects")]
    public async Task<IActionResult> GetProjectsAsync(CancellationToken ct)
    {
        var result = await salesService.GetAvailableProjectsAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<SalesProjectDto>>.Success(result));
    }

    [HttpGet("clients")]
    public async Task<IActionResult> GetClientsAsync(CancellationToken ct)
    {
        var result = await salesService.GetClientsAsync(CurrentUserId, ct);
        return Ok(ApiResponse<IReadOnlyList<SalesClientDto>>.Success(result));
    }

    [HttpGet("contracts")]
    public async Task<IActionResult> GetContractsAsync(CancellationToken ct)
    {
        var result = await salesService.GetContractsAsync(CurrentUserId, ct);
        return Ok(ApiResponse<IReadOnlyList<SalesContractDto>>.Success(result));
    }

    [HttpGet("contracts/{id:guid}")]
    public async Task<IActionResult> GetContractDetailAsync(Guid id, CancellationToken ct)
    {
        var result = await salesService.GetContractDetailAsync(CurrentUserId, id, ct);
        return Ok(ApiResponse<SalesContractDetailDto>.Success(result));
    }

    [HttpPost("contracts")]
    public async Task<IActionResult> InitiateContractAsync([FromBody] InitiateContractRequest request, CancellationToken ct)
    {
        var result = await salesService.InitiateContractAsync(CurrentUserId, request, ct);
        return Ok(ApiResponse<InitiateContractResult>.Success(result));
    }

    [HttpPost("contracts/{id:guid}/cancel")]
    public async Task<IActionResult> CancelContractAsync(Guid id, CancellationToken ct)
    {
        await salesService.CancelContractAsync(CurrentUserId, id, ct);
        return Ok(ApiResponse<object>.Success(null!));
    }

    [HttpDelete("contracts/{id:guid}")]
    public async Task<IActionResult> DeleteContractAsync(Guid id, CancellationToken ct)
    {
        await salesService.DeleteContractAsync(CurrentUserId, id, ct);
        return Ok(ApiResponse<object>.Success(null!));
    }

    [HttpPatch("contracts/{id:guid}/client")]
    public async Task<IActionResult> ChangeContractClientAsync(Guid id, [FromBody] ChangeContractClientRequest request, CancellationToken ct)
    {
        await salesService.ChangeContractClientAsync(CurrentUserId, id, request.NewClientId, ct);
        return Ok(ApiResponse<object>.Success(null!));
    }

    [HttpGet("performance")]
    public async Task<IActionResult> GetPerformanceAsync(CancellationToken ct)
    {
        var result = await salesService.GetPerformanceAsync(CurrentUserId, ct);
        return Ok(ApiResponse<SalesPerformanceDto>.Success(result));
    }
}
