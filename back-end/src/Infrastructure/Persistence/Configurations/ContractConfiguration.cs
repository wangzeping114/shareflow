using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareFlow.Domain.Entities;
using ShareFlow.Domain.Enums;
namespace ShareFlow.Infrastructure.Persistence.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("contracts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ContractNo)
            .HasColumnName("contract_no")
            .HasDefaultValueSql("nextval('contract_no_seq')")
            .ValueGeneratedOnAdd();
        builder.Property(x => x.ProjectId).HasColumnName("project_id");
        builder.Property(x => x.SlotId).HasColumnName("slot_id");
        builder.Property(x => x.InvestorUserId).HasColumnName("investor_user_id");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(v => v.ToString(), v => Enum.Parse<ContractStatus>(v))
            .HasMaxLength(20);

        builder.Property(x => x.TemplateType)
            .HasColumnName("template_type")
            .HasMaxLength(30);

        builder.Property(x => x.SignToken).HasColumnName("sign_token").HasMaxLength(32);
        builder.Property(x => x.SignTokenExpiresAt).HasColumnName("sign_token_expires_at");
        builder.Property(x => x.SignatureDataUrl).HasColumnName("signature_data_url").HasColumnType("text");
        builder.Property(x => x.SignedAt).HasColumnName("signed_at");
        builder.Property(x => x.PdfStoragePath).HasColumnName("pdf_storage_path").HasMaxLength(500);
        builder.Property(x => x.ContractSnapshot).HasColumnName("contract_snapshot").HasColumnType("text");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.SignToken).IsUnique().HasFilter("sign_token IS NOT NULL")
            .HasDatabaseName("ix_contracts_sign_token");
        builder.HasIndex(x => x.ContractNo).IsUnique().HasDatabaseName("ix_contracts_contract_no");
        builder.HasIndex(x => x.ProjectId).HasDatabaseName("ix_contracts_project_id");
        builder.HasIndex(x => x.InvestorUserId).HasDatabaseName("ix_contracts_investor_user_id");
        builder.HasIndex(x => x.Status).HasDatabaseName("ix_contracts_status");

        builder.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Slot).WithMany().HasForeignKey(x => x.SlotId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.InvestorUser).WithMany().HasForeignKey(x => x.InvestorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
