using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BAR.Database.Entities;

namespace BAR.Database.Configurations;

public class MoodTrackingConfiguration : IEntityTypeConfiguration<MoodTracking>
{
    public void Configure(EntityTypeBuilder<MoodTracking> builder)
    {
        builder.ToTable("mood_tracking");
        builder.HasKey(m => m.AccountId);
        builder.Property(m => m.AccountId).HasColumnName("account_id").HasMaxLength(20);
        builder.Property(m => m.MoodLevel).HasColumnName("mood_level").HasMaxLength(20).HasDefaultValue("normal");
        builder.Property(m => m.ConsecutiveSimilarOrders).HasColumnName("consecutive_similar_orders").HasDefaultValue(0);
        builder.Property(m => m.LastDrink).HasColumnName("last_drink").HasMaxLength(100);
        builder.Property(m => m.LastOrderTime).HasColumnName("last_order_time");

        builder.HasOne(m => m.Account).WithOne(a => a.MoodTracking);
    }
}
