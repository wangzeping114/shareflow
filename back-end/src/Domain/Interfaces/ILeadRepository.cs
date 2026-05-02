using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Domain.Interfaces;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Lead> Items, int Total)> GetPagedAsync(
        Guid salesOwnerId,
        LeadStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);
    Task<int> CountByStatusAsync(Guid salesOwnerId, LeadStatus? status = null, CancellationToken ct = default);
    Task AddAsync(Lead lead, CancellationToken ct = default);
    Task UpdateAsync(Lead lead, CancellationToken ct = default);
}
