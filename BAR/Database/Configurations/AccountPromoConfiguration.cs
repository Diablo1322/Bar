using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BAR.Database.Entities;

namespace BAR.Database.Configurations;

public class AccountPromoConfiguration : IEntityTypeConfiguration<AccountPromo>
{
    public void Configure(EntityTypeBuilder<AccountPromo> builder)
    {
        builder.ToTable("account_promos");
        builder.HasKey(ap => ap.Id);
        builder.Property(ap => ap.Id).HasColumnName("id").UseIdentityByDefaultColumn();
        builder.Property(ap => ap.AccountId).HasColumnName("account_id").HasMaxLength(20).IsRequired();
        builder.Property(ap => ap.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(ap => ap.UsedAt).HasColumnName("used_at");

        builder.HasOne(ap => ap.Account)
            .WithMany()
            .HasForeignKey(ap => ap.AccountId);

        builder.HasIndex(ap => new { ap.AccountId, ap.Code }).IsUnique();
    }
}