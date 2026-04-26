using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(256).IsRequired();
        builder.Property(x => x.Username).HasColumnName("username").HasMaxLength(64).IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(256).IsRequired();
        builder.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.BackendRoleId).HasColumnName("backend_role_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.Username).IsUnique();

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}