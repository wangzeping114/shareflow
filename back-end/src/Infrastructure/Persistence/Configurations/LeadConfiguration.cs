using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Infrastructure.Persistence.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("leads");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ContactInfo).HasColumnName("contact_info").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(200);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(v => v.ToString(), v => Enum.Parse<LeadStatus>(v))
            .HasMaxLength(20);

        builder.Property(x => x.SalesOwnerId).HasColumnName("sales_owner_id");
        builder.Property(x => x.ClientUserId).HasColumnName("client_user_id");
        builder.Property(x => x.ClientInitialPassword).HasColumnName("client_initial_password").HasMaxLength(64);
        builder.Property(x => x.Notes).HasColumnName("notes").HasColumnType("text");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.SalesOwnerId).HasDatabaseName("ix_leads_sales_owner_id");
        builder.HasIndex(x => x.Status).HasDatabaseName("ix_leads_status");
        builder.HasIndex(x => x.ClientUserId).HasDatabaseName("ix_leads_client_user_id");
    }
}
