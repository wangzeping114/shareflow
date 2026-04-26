using ShareFlow.Application.Admin.DTOs;

namespace ShareFlow.Application.Admin.Interfaces;

public interface IRoleService
{
    Task<IReadOnlyList<BackendRoleDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<BackendRoleDto> CreateAsync(CreateBackendRoleRequest request, CancellationToken cancellationToken = default);

    Task<BackendRoleDto> UpdatePermissionsAsync(Guid id, UpdateBackendRolePermissionsRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task AssignRoleToUserAsync(Guid userId, AssignBackendRoleRequest request, CancellationToken cancellationToken = default);
}