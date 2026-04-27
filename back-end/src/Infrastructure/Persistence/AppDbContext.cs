using Microsoft.EntityFrameworkCore;
using ShareFlow.Domain.Entities;

namespace ShareFlow.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<BackendRole> BackendRoles => Set<BackendRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<VideoProject> VideoProjects => Set<VideoProject>();
    public DbSet<ProjectSlot> ProjectSlots => Set<ProjectSlot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // EF Core Configurations 从 Infrastructure.Persistence.Configurations 自动加载
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        // 软删除全局查询过滤器
        modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
    }
}
