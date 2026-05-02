using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShareFlow.Application.Auth.Interfaces;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;
using ShareFlow.Infrastructure.Persistence;

namespace ShareFlow.Infrastructure.Services;

public class IdentitySeedService(
    AppDbContext dbContext,
    IUserRepository userRepository,
    IConfiguration configuration,
    ILogger<IdentitySeedService> logger) : IIdentitySeedService
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);

            await SeedAdminAsync(cancellationToken);
            await SeedSalesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Identity seed skipped because database is unavailable.");
        }
    }

    private async Task SeedAdminAsync(CancellationToken cancellationToken)
    {
        var email = configuration["SeedAdmin:Email"] ?? "admin@shareflow.local";
        var username = configuration["SeedAdmin:Username"] ?? "admin";
        var password = configuration["SeedAdmin:Password"] ?? "Admin@123456";
        var displayName = configuration["SeedAdmin:DisplayName"] ?? "ShareFlow Super Admin";

        var exists = await userRepository.ExistsByEmailAsync(email, cancellationToken)
            || await userRepository.ExistsByUsernameAsync(username, cancellationToken);
        if (exists)
        {
            return;
        }

        var user = User.Create(username, email, password, displayName, UserRole.SuperAdmin);
        await userRepository.AddAsync(user, cancellationToken);
    }

    private async Task SeedSalesAsync(CancellationToken cancellationToken)
    {
        var email = configuration["SeedSales:Email"] ?? "sales@shareflow.local";
        var username = configuration["SeedSales:Username"] ?? "sales";
        var password = configuration["SeedSales:Password"] ?? "Sales@123456";
        var displayName = configuration["SeedSales:DisplayName"] ?? "ShareFlow Sales";

        var exists = await userRepository.ExistsByEmailAsync(email, cancellationToken)
            || await userRepository.ExistsByUsernameAsync(username, cancellationToken);
        if (exists)
        {
            return;
        }

        var user = User.Create(username, email, password, displayName, UserRole.Sales);
        await userRepository.AddAsync(user, cancellationToken);
    }
}