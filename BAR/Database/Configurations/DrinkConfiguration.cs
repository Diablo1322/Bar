using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BAR.Database.Entities;

namespace BAR.Database.Configurations;

public class DrinkConfiguration : IEntityTypeConfiguration<Drink>
{
    public void Configure(EntityTypeBuilder<Drink> builder)
    {
        builder.ToTable("drinks");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id").UseIdentityByDefaultColumn();
        builder.Property(d => d.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(d => d.BasePrice).HasColumnName("base_price").IsRequired();
        builder.Property(d => d.IsNight).HasColumnName("is_night").HasDefaultValue(false);
        builder.Property(d => d.IsHidden).HasColumnName("is_hidden").HasDefaultValue(false);
        builder.Property(d => d.MoodPriceModifier).HasColumnName("mood_price_modifier").HasDefaultValue(0);
        builder.Property(d => d.MinDrinkCount).HasColumnName("min_drink_count").HasDefaultValue(0);
        builder.HasIndex(d => d.Name).IsUnique();
    }
}