using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Infrastructure.Persistence.Configurations;

public class DividendRecordConfiguration : IEntityTypeConfiguration<DividendRecord>
{
    public void Configure(EntityTypeBuilder<DividendRecord> builder)
    {
        builder.ToTable("dividend_records");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProjectId).HasColumnName("project_id");
        builder.Property(x => x.SlotId).HasColumnName("slot_id");
        builder.Property(x => x.InvestorUserId).HasColumnName("investor_user_id");
        builder.Property(x => x.PlatformRevenueId).HasColumnName("platform_revenue_id");
        builder.Property(x => x.RevenueAmount).HasColumnName("revenue_amount").HasPrecision(15, 2);
        builder.Property(x => x.SharePermille).HasColumnName("share_permille").HasPrecision(8, 4);
        builder.Property(x => x.DividendAmount).HasColumnName("dividend_amount").HasPrecision(15, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(10);
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(v => v.ToString(), v => Enum.Parse<DividendStatus>(v))
            .HasMaxLength(20);
        builder.Property(x => x.CalculatedAt).HasColumnName("calculated_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.ProjectId).HasDatabaseName("ix_dividend_records_project_id");
        builder.HasIndex(x => x.InvestorUserId).HasDatabaseName("ix_dividend_records_investor_user_id");
        builder.HasIndex(x => x.PlatformRevenueId).HasDatabaseName("ix_dividend_records_revenue_id");
        builder.HasIndex(x => x.Status).HasDatabaseName("ix_dividend_records_status");

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Slot)
            .WithMany()
            .HasForeignKey(x => x.SlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.InvestorUser)
            .WithMany()
            .HasForeignKey(x => x.InvestorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PlatformRevenue)
            .WithMany()
            .HasForeignKey(x => x.PlatformRevenueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
