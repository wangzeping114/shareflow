namespace ShareFlow.Application.Auth.Interfaces;

public interface IIdentitySeedService
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}