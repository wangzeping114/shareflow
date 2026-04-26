using ShareFlow.Application.Auth.Interfaces;
using ShareFlow.Domain.Common;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;

namespace ShareFlow.Infrastructure.Services;

public class AccountProvisioningService(IUserRepository userRepository) : IAccountProvisioningService
{
    public async Task<(string email, string initialPassword)> ProvisionClientAccountAsync(
        string clientName,
        string clientEmail,
        Guid contractId,
        CancellationToken cancellationToken = default)
    {
        if (await userRepository.ExistsByEmailAsync(clientEmail, cancellationToken))
        {
            throw new BusinessException("客户账号已存在。", 400);
        }

        var initialPassword = $"SF{contractId.ToString("N")[..8]}!";
        var username = clientEmail.Split('@')[0].ToLowerInvariant();
        var user = User.Create(username, clientEmail, initialPassword, clientName, UserRole.Client);
        user.MarkPendingSetup();
        await userRepository.AddAsync(user, cancellationToken);

        return (user.Email, initialPassword);
    }
}