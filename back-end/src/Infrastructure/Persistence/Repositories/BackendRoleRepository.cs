using Microsoft.EntityFrameworkCore;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Infrastructure.Persistence.Repositories;

public class BackendRoleRepository(AppDbContext dbContext) : IBackendRoleRepository
{
    public async Task<BackendRole?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.BackendRoles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<BackendRole>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.BackendRoles
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(BackendRole role, CancellationToken cancellationToken = default)
    {
        await dbContext.BackendRoles.AddAsync(role, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(BackendRole role, CancellationToken cancellationToken = default)
    {
        dbContext.BackendRoles.Update(role);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.BackendRoles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        dbContext.BackendRoles.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}