using MapsterMapper;
using ShareFlow.Application.Admin.DTOs;
using ShareFlow.Application.Admin.Interfaces;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Application.Admin;

public class RoleService(
    IBackendRoleRepository backendRoleRepository,
    IUserRepository userRepository,
    IMapper mapper) : IRoleService
{
    public async Task<IReadOnlyList<BackendRoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await backendRoleRepository.GetAllAsync(cancellationToken);
        return mapper.Map<IReadOnlyList<BackendRoleDto>>(roles);
    }

    public async Task<BackendRoleDto> CreateAsync(CreateBackendRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BusinessException("角色名称不能为空。", 400);
        }

        var role = BackendRole.Create(request.Name, request.Permissions);
        await backendRoleRepository.AddAsync(role, cancellationToken);
        return mapper.Map<BackendRoleDto>(role);
    }

    public async Task<BackendRoleDto> UpdatePermissionsAsync(Guid id, UpdateBackendRolePermissionsRequest request, CancellationToken cancellationToken = default)
    {
        var role = await backendRoleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException("角色不存在。", 404);

        role.SetPermissions(request.Permissions);
        await backendRoleRepository.UpdateAsync(role, cancellationToken);

        return mapper.Map<BackendRoleDto>(role);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await backendRoleRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task AssignRoleToUserAsync(Guid userId, AssignBackendRoleRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new BusinessException("用户不存在。", 404);

        if (request.BackendRoleId.HasValue)
        {
            var role = await backendRoleRepository.GetByIdAsync(request.BackendRoleId.Value, cancellationToken)
                ?? throw new BusinessException("后台角色不存在。", 404);

            user.AssignRole(UserRole.BackendCustom, role.Id);
        }
        else
        {
            user.AssignRole(UserRole.Client);
        }

        await userRepository.UpdateAsync(user, cancellationToken);
    }
}