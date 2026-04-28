using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareFlow.Api.Filters;
using ShareFlow.Application.Common;
using ShareFlow.Application.Project.DTOs;
using ShareFlow.Application.Project.Interfaces;
using ShareFlow.Domain.Enums;
using System.Security.Claims;

namespace ShareFlow.Api.Controllers.Admin;

[ApiController]
[Route("v1/admin/projects")]
[Authorize]
public class ProjectController(IProjectService projectService) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [Permission("project.read")]
    public async Task<IActionResult> GetListAsync([FromQuery] ProjectQueryRequest query, CancellationToken ct)
    {
        var result = await projectService.GetListAsync(query, ct);
        return Ok(ApiResponse<PagedResult<ProjectDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [Permission("project.read")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var result = await projectService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ProjectDetailDto>.Success(result));
    }

    [HttpPost]
    [Permission("project.write")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProjectRequest request, CancellationToken ct)
    {
        var id = await projectService.CreateAsync(request, CurrentUserId, ct);
        return Ok(ApiResponse<object>.Success(new { id }));
    }

    [HttpPut("{id:guid}")]
    [Permission("project.write")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken ct)
    {
        await projectService.UpdateAsync(id, request, CurrentUserId, ct);
        return Ok(ApiResponse<object>.Success(new { }));
    }

    [HttpPost("{id:guid}/status")]
    [Permission("project.write")]
    public async Task<IActionResult> ChangeStatusAsync(Guid id, [FromBody] ChangeProjectStatusRequest request, CancellationToken ct)
    {
        await projectService.ChangeStatusAsync(id, request, CurrentUserId, ct);
        return Ok(ApiResponse<object>.Success(new { }));
    }

    [HttpGet("{id:guid}/slots")]
    [Permission("project.read")]
    public async Task<IActionResult> GetSlotsAsync(Guid id, CancellationToken ct)
    {
        var detail = await projectService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<IReadOnlyList<SlotDto>>.Success(detail.Slots));
    }

    [HttpPost("{id:guid}/slots")]
    [Permission("project.write")]
    public async Task<IActionResult> AddSlotAsync(Guid id, [FromBody] AddSlotRequest request, CancellationToken ct)
    {
        var slot = await projectService.AddSlotAsync(id, request, ct);
        return Ok(ApiResponse<SlotDto>.Success(slot));
    }

    [HttpPatch("{id:guid}/slots/{slotId:guid}/contract-months")]
    [Permission("project.write")]
    public async Task<IActionResult> UpdateSlotContractMonthsAsync(
        Guid id, Guid slotId,
        [FromBody] UpdateSlotContractMonthsRequest request,
        CancellationToken ct)
    {
        var slot = await projectService.UpdateSlotContractMonthsAsync(id, slotId, request, ct);
        return Ok(ApiResponse<SlotDto>.Success(slot));
    }

    [HttpPatch("{id:guid}/slots/batch")]
    [Permission("project.write")]
    public async Task<IActionResult> BatchUpdateSlotsAsync(
        Guid id,
        [FromBody] BatchUpdateSlotsRequest request,
        CancellationToken ct)
    {
        var slots = await projectService.BatchUpdateSlotsAsync(id, request, ct);
        return Ok(ApiResponse<IReadOnlyList<SlotDto>>.Success(slots));
    }
}
