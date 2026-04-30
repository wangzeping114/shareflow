using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Infrastructure.Persistence.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("wallet_transactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(x => x.WalletId).HasColumnName("wallet_id");
        builder.Property(x => x.InvestorUserId).HasColumnName("investor_user_id");
        builder.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Direction).HasColumnName("direction").HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(15, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(10);
        builder.Property(x => x.BalanceBefore).HasColumnName("balance_before").HasPrecision(15, 2);
        builder.Property(x => x.BalanceAfter).HasColumnName("balance_after").HasPrecision(15, 2);
        builder.Property(x => x.Remark).HasColumnName("remark").HasMaxLength(500);
        builder.Property(x => x.ReferenceId).HasColumnName("reference_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(x => x.InvestorUserId);
        builder.HasIndex(x => x.WalletId);
    }
}
