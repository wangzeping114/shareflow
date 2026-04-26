using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using ShareFlow.Infrastructure.Persistence;

namespace ShareFlow.Migrator;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        var connectionString = config.GetConnectionString("Default")
            ?? "Host=localhost;Port=5433;Database=shareflow_overseas;Username=shareflow;Password=shareflow";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString,
            b => b.MigrationsAssembly("ShareFlow.Migrator"));

        return new AppDbContext(optionsBuilder.Options);
    }
}
