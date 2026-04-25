using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BAR.Database.Entities;

namespace BAR.Database.Configurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("profiles");
        builder.HasKey(p => p.AccountId);
        builder.Property(p => p.AccountId).HasColumnName("account_id").HasMaxLength(20);
        builder.Property(p => p.Rank).HasColumnName("rank").HasMaxLength(50).HasDefaultValue("Новичок");
        builder.Property(p => p.TotalOrders).HasColumnName("total_orders").HasDefaultValue(0);
        builder.Property(p => p.UniqueDrinks).HasColumnName("unique_drinks").HasDefaultValue(0);
        builder.Property(p => p.FavoriteDrink).HasColumnName("favorite_drink").HasMaxLength(100);
        builder.Property(p => p.BarClosed).HasColumnName("bar_closed").HasDefaultValue(false);

        builder.HasOne(p => p.Account).WithOne(a => a.Profile);
    }
}
