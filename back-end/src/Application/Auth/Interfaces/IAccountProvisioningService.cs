namespace ShareFlow.Application.Auth.Interfaces;

public interface IAccountProvisioningService
{
    Task<(string email, string initialPassword)> ProvisionClientAccountAsync(
        string clientName,
        string clientEmail,
        Guid contractId,
        CancellationToken cancellationToken = default);
}