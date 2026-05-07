using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Infrastructure.Persistence.Configurations;

public class ProjectSlotConfiguration : IEntityTypeConfiguration<ProjectSlot>
{
    public void Configure(EntityTypeBuilder<ProjectSlot> builder)
    {
        builder.ToTable("project_slots");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProjectId).HasColumnName("project_id");
        builder.Property(x => x.SlotNumber).HasColumnName("slot_number").HasDefaultValue(0);
        builder.Property(x => x.Alias).HasColumnName("alias").HasMaxLength(50).IsRequired(false);
        builder.Property(x => x.SharePermille)
            .HasColumnName("share_permille")
            .HasPrecision(8, 4);
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(v => v.ToString(), v => Enum.Parse<SlotStatus>(v))
            .HasMaxLength(20);
        builder.Property(x => x.ClientUserId).HasColumnName("client_user_id");
        builder.Property(x => x.ContractMonths).HasColumnName("contract_months").HasDefaultValue(12);
        builder.Property(x => x.TemplateType).HasColumnName("template_type").HasMaxLength(30).HasDefaultValue("OverseasEnglish");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.ProjectId).HasDatabaseName("ix_project_slots_project_id");
        builder.HasIndex(x => x.ClientUserId).HasDatabaseName("ix_project_slots_client_user_id");
    }
}
