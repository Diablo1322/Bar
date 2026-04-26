using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BAR.Database.Entities;

namespace BAR.Database.Configurations;

public class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        builder.ToTable("promo_codes");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").UseIdentityByDefaultColumn();
        builder.Property(p => p.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(p => p.MaxUses).HasColumnName("max_uses");
        builder.Property(p => p.UsesLeft).HasColumnName("uses_left");
        builder.Property(p => p.BonusBalance).HasColumnName("bonus_balance").HasDefaultValue(0);
        builder.Property(p => p.MoodEffect).HasColumnName("mood_effect").HasMaxLength(20);
        builder.Property(p => p.NightAccess).HasColumnName("night_access").HasDefaultValue(false);
        builder.Property(p => p.FreeDrink).HasColumnName("free_drink").HasDefaultValue(false);
        builder.Property(p => p.IsEnabled).HasColumnName("is_enabled").HasDefaultValue(true);
        builder.HasIndex(p => p.Code).IsUnique();
    }
}