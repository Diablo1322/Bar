using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BAR.Database.Entities;

namespace BAR.Database.Configurations;

public class BalanceConfiguration : IEntityTypeConfiguration<Balance>
{
    public void Configure(EntityTypeBuilder<Balance> builder)
    {
        builder.ToTable("balances");
        builder.HasKey(b => b.AccountId);
        builder.Property(b => b.AccountId).HasColumnName("account_id").HasMaxLength(20);
        builder.Property(b => b.Amount).HasColumnName("balance").HasDefaultValue(100);

        builder.HasOne(b => b.Account).WithOne(a => a.Balance);
    }
}
