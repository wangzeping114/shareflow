using ShareFlow.Application.Common;
using ShareFlow.Application.Project.DTOs;

namespace ShareFlow.Application.Project.Interfaces;

public interface IProjectService
{
    Task<PagedResult<ProjectDto>> GetListAsync(ProjectQueryRequest query, CancellationToken ct = default);
    Task<ProjectDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CreateAsync(CreateProjectRequest request, Guid operatorId, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateProjectRequest request, Guid operatorId, CancellationToken ct = default);
    Task ChangeStatusAsync(Guid id, ChangeProjectStatusRequest request, Guid operatorId, CancellationToken ct = default);
    Task<SlotDto> AddSlotAsync(Guid projectId, AddSlotRequest request, CancellationToken ct = default);
    Task<SlotDto> UpdateSlotContractMonthsAsync(Guid projectId, Guid slotId, UpdateSlotContractMonthsRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<SlotDto>> BatchUpdateSlotsAsync(Guid projectId, BatchUpdateSlotsRequest request, CancellationToken ct = default);
}
