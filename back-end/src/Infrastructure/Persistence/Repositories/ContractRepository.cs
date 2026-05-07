using Microsoft.EntityFrameworkCore;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;
using ShareFlow.Infrastructure.Persistence;

namespace ShareFlow.Infrastructure.Repositories;

public class ContractRepository(AppDbContext db) : IContractRepository
{
    public async Task<Contract?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Contracts.FindAsync([id], ct);

    public async Task<Contract?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await db.Contracts
            .Include(c => c.Project)
            .Include(c => c.Slot)
            .Include(c => c.InvestorUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Contract?> GetBySignTokenAsync(string token, CancellationToken ct = default)
        => await db.Contracts
            .Include(c => c.Project)
            .Include(c => c.Slot)
            .Include(c => c.InvestorUser)
            .FirstOrDefaultAsync(c => c.SignToken == token, ct);

    public async Task<(IReadOnlyList<Contract> Items, int Total)> GetPagedAsync(
        Guid? projectId, ContractStatus? status, string? projectTitle, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Contracts
            .Include(c => c.Project)
            .Include(c => c.InvestorUser)
            .AsNoTracking()
            .AsQueryable();

        if (projectId.HasValue)
            query = query.Where(c => c.ProjectId == projectId.Value);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(projectTitle))
            query = query.Where(c => c.Project != null &&
                EF.Functions.ILike(c.Project.Title, $"%{projectTitle.Trim()}%"));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<Contract>> GetSignedAsync(CancellationToken ct = default)
    {
        return await db.Contracts
            .AsNoTracking()
            .Where(c => c.Status == ContractStatus.Signed || c.Status == ContractStatus.Executed)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Contract>> GetByClientIdAsync(Guid clientUserId, CancellationToken ct = default)
    {
        return await db.Contracts
            .Include(c => c.Project)
            .Include(c => c.Slot)
            .AsNoTracking()
            .Where(c => c.InvestorUserId == clientUserId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Contract contract, CancellationToken ct = default)
    {
        await db.Contracts.AddAsync(contract, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Contract contract, CancellationToken ct = default)
    {
        db.Contracts.Update(contract);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Contract contract, CancellationToken ct = default)
    {
        db.Contracts.Remove(contract);
        await db.SaveChangesAsync(ct);
    }

    public async Task<Contract?> GetActiveUnsignedAsync(Guid projectId, Guid slotId, Guid investorUserId, CancellationToken ct = default)
        => await db.Contracts
            .AsNoTracking()
            .Where(c => c.ProjectId == projectId
                     && c.SlotId == slotId
                     && c.InvestorUserId == investorUserId
                     && (c.Status == ContractStatus.Draft || c.Status == ContractStatus.Sent))
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);
}
