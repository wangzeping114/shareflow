using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareFlow.Domain.Entities;

namespace ShareFlow.Infrastructure.Persistence.Configurations;

public class WithdrawalRequestConfiguration : IEntityTypeConfiguration<WithdrawalRequest>
{
    public void Configure(EntityTypeBuilder<WithdrawalRequest> builder)
    {
        builder.ToTable("withdrawal_requests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.InvestorUserId).HasColumnName("investor_user_id");
        builder.Property(x => x.WalletId).HasColumnName("wallet_id");
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(15, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(10);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(500);
        builder.Property(x => x.RejectReason).HasColumnName("reject_reason").HasMaxLength(500);
        builder.Property(x => x.ProcessedByUserId).HasColumnName("processed_by_user_id");
        builder.Property(x => x.ProcessedAt).HasColumnName("processed_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(x => x.InvestorUserId);
        builder.HasIndex(x => x.Status);
    }
}
