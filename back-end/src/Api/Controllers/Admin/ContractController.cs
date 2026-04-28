using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareFlow.Api.Filters;
using ShareFlow.Application.Common;
using ShareFlow.Application.Contracts.DTOs;
using ShareFlow.Application.Contracts.Interfaces;
using ShareFlow.Domain.Enums;
using System.Security.Claims;

namespace ShareFlow.Api.Controllers.Admin;

[ApiController]
[Route("v1/admin/contracts")]
[Authorize]
public class ContractController(
    IContractService contractService,
    IContractTemplateService templateService) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [Permission("contract.read")]
    public async Task<IActionResult> GetListAsync([FromQuery] ContractQueryRequest query, CancellationToken ct)
    {
        var result = await contractService.GetListAsync(query, ct);
        return Ok(ApiResponse<PagedResult<ContractDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [Permission("contract.read")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var result = await contractService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ContractDetailDto>.Success(result));
    }

    [HttpPost]
    [Permission("contract.write")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateContractRequest request, CancellationToken ct)
    {
        var id = await contractService.CreateAsync(request, CurrentUserId, ct);
        return Ok(ApiResponse<object>.Success(new { id }));
    }

    [HttpPost("{id:guid}/send")]
    [Permission("contract.write")]
    public async Task<IActionResult> GenerateSignLinkAsync(Guid id, CancellationToken ct)
    {
        var result = await contractService.GenerateSignLinkAsync(id, ct);
        return Ok(ApiResponse<GenerateSignLinkResult>.Success(result));
    }

    /// <summary>获取指定类型的合同原始模板文本（含占位符）</summary>
    [HttpGet("template/preview")]
    [Permission("contract.read")]
    public IActionResult GetTemplatePreview([FromQuery] string type = "OverseasEnglish")
    {
        var raw = templateService.GetRaw(type);
        return Ok(ApiResponse<object>.Success(new { type, content = raw }));
    }
}
