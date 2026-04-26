using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareFlow.Domain.Entities;

namespace ShareFlow.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Token).HasColumnName("token").HasMaxLength(256).IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        builder.Property(x => x.IsRevoked).HasColumnName("is_revoked");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.Token).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}