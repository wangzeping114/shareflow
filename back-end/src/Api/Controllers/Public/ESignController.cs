using Microsoft.AspNetCore.Mvc;
using ShareFlow.Application.Common;
using ShareFlow.Application.Contracts.DTOs;
using ShareFlow.Application.Contracts.Interfaces;

namespace ShareFlow.Api.Controllers.Public;

[ApiController]
[Route("v1/public/esign")]
public class ESignController(IESignService eSignService) : ControllerBase
{
    [HttpGet("{token}")]
    public async Task<IActionResult> GetPreviewAsync(string token, CancellationToken ct)
    {
        var result = await eSignService.GetPreviewAsync(token, ct);
        return Ok(ApiResponse<ContractPreviewDto>.Success(result));
    }

    [HttpPost("{token}")]
    public async Task<IActionResult> SignAsync(string token, [FromBody] SubmitSignatureRequest request, CancellationToken ct)
    {
        var result = await eSignService.SignAsync(token, request.SignatureDataUrl, ct);
        return Ok(ApiResponse<SignContractResult>.Success(result));
    }
}
