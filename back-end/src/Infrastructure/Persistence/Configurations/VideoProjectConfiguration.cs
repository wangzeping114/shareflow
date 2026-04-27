using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Infrastructure.Persistence.Configurations;

public class VideoProjectConfiguration : IEntityTypeConfiguration<VideoProject>
{
    public void Configure(EntityTypeBuilder<VideoProject> builder)
    {
        builder.ToTable("video_projects");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(x => x.PlatformName).HasColumnName("platform_name").HasMaxLength(500).IsRequired();
        builder.Property(x => x.SlotMode)
            .HasColumnName("slot_mode")
            .HasConversion(v => v.ToString(), v => Enum.Parse<ProjectSlotMode>(v))
            .HasMaxLength(20);
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(v => v.ToString(), v => Enum.Parse<ProjectStatus>(v))
            .HasMaxLength(20);
        builder.Property(x => x.TotalSlots).HasColumnName("total_slots");
        builder.Property(x => x.TotalInvestment).HasColumnName("total_investment").HasPrecision(15, 2);
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.Status).HasDatabaseName("ix_video_projects_status");
        builder.HasIndex(x => x.CreatedByUserId).HasDatabaseName("ix_video_projects_created_by");

        builder.HasQueryFilter(x => !x.IsDeleted);

        // 导航属性（Slots 通过 ProjectSlotConfiguration 配置的 FK 关联）
        builder.Ignore(x => x.FilledSlots);
        builder.Ignore(x => x.ReservedSlots);
        builder.Ignore(x => x.AvailableSlots);
        builder.Ignore(x => x.HasAvailableSlot);
    }
}
