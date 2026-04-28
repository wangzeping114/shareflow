using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Infrastructure.Persistence.Configurations;

public class PlatformRevenueConfiguration : IEntityTypeConfiguration<PlatformRevenue>
{
    public void Configure(EntityTypeBuilder<PlatformRevenue> builder)
    {
        builder.ToTable("platform_revenues");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProjectId).HasColumnName("project_id");
        builder.Property(x => x.PlatformName).HasColumnName("platform_name").HasMaxLength(50);
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(15, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(10);
        builder.Property(x => x.RevenueDate).HasColumnName("revenue_date");

        builder.Property(x => x.ImportSource)
            .HasColumnName("import_source")
            .HasConversion(v => v.ToString(), v => Enum.Parse<RevenueImportSource>(v))
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(v => v.ToString(), v => Enum.Parse<RevenueStatus>(v))
            .HasMaxLength(20);

        builder.Property(x => x.ScreenshotStoragePath).HasColumnName("screenshot_storage_path").HasMaxLength(500);
        builder.Property(x => x.AiRawResult).HasColumnName("ai_raw_result").HasColumnType("text");
        builder.Property(x => x.AiConfidence).HasColumnName("ai_confidence").HasPrecision(4, 3);
        builder.Property(x => x.VerifiedByUserId).HasColumnName("verified_by_user_id");
        builder.Property(x => x.VerifiedAt).HasColumnName("verified_at");
        builder.Property(x => x.RejectReason).HasColumnName("reject_reason").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.ProjectId).HasDatabaseName("ix_platform_revenues_project_id");
        builder.HasIndex(x => x.Status).HasDatabaseName("ix_platform_revenues_status");
        builder.HasIndex(x => x.RevenueDate).HasDatabaseName("ix_platform_revenues_date");

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
