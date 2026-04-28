using Microsoft.EntityFrameworkCore;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(x => x.BackendRole)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(x => x.BackendRole)
            .FirstOrDefaultAsync(x => x.Username == username.Trim().ToLower(), cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(x => x.BackendRole)
            .FirstOrDefaultAsync(x => x.Email == email.Trim().ToLower(), cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.AsNoTracking().AnyAsync(x => x.Username == username.Trim().ToLower(), cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.AsNoTracking().AnyAsync(x => x.Email == email.Trim().ToLower(), cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId && x.BackendRoleId.HasValue)
            .SelectMany(x => x.BackendRole!.Permissions)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(x => x.Role == role && !x.IsDeleted)
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);
    }
}