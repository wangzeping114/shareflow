using Mapster;
using ShareFlow.Application.Admin.DTOs;
using ShareFlow.Application.Auth.DTOs;
using ShareFlow.Domain.Entities;

namespace ShareFlow.Application.Auth;

public class AuthMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.RoleName, src => src.Role.ToString());

        config.NewConfig<BackendRole, BackendRoleDto>();
    }
}