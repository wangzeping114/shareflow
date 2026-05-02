using ShareFlow.Application.Admin.DTOs;

namespace ShareFlow.Application.Admin.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default);
}
