using Microsoft.EntityFrameworkCore;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(AppDbContext dbContext) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
    }

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        await dbContext.RefreshTokens.AddAsync(token, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        dbContext.RefreshTokens.Update(token);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await dbContext.RefreshTokens.Where(x => x.UserId == userId && !x.IsRevoked).ToListAsync(cancellationToken);
        foreach (var token in tokens)
        {
            token.Revoke();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}