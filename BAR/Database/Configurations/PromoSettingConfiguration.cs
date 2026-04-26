using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BAR.Database.Entities;

namespace BAR.Database.Configurations;

public class PromoSettingConfiguration : IEntityTypeConfiguration<PromoSetting>
{
    public void Configure(EntityTypeBuilder<PromoSetting> builder)
    {
        builder.ToTable("promo_settings");
        builder.HasKey(ps => ps.Id);
        builder.Property(ps => ps.Id).HasColumnName("id");
        builder.Property(ps => ps.PromoEnabled).HasColumnName("promo_enabled").HasDefaultValue(true);
    }
}