using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareFlow.Domain.Entities;

namespace ShareFlow.Infrastructure.Persistence.Configurations;

public class BackendRoleConfiguration : IEntityTypeConfiguration<BackendRole>
{
    public void Configure(EntityTypeBuilder<BackendRole> builder)
    {
        builder.ToTable("backend_roles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property<List<string>>("_permissions")
            .HasColumnName("permissions")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
    }
}