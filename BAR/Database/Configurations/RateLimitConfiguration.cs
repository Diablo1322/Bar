using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BAR.Database.Entities;

namespace BAR.Database.Configurations;

public class RateLimitConfiguration : IEntityTypeConfiguration<RateLimit>
{
    public void Configure(EntityTypeBuilder<RateLimit> builder)
    {
        builder.ToTable("rate_limits");
        builder.HasKey(rl => rl.AccountId);
        builder.Property(rl => rl.AccountId).HasColumnName("account_id").HasMaxLength(20);
        builder.Property(rl => rl.RequestCount).HasColumnName("request_count").HasDefaultValue(0);
        builder.Property(rl => rl.WindowStart).HasColumnName("window_start");

        builder.HasOne(rl => rl.Account).WithOne(a => a.RateLimit);
    }
}
