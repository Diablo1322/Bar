using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BAR.Database.Entities;

namespace BAR.Database.Configurations;

public class OrderSequenceConfiguration : IEntityTypeConfiguration<OrderSequence>
{
    public void Configure(EntityTypeBuilder<OrderSequence> builder)
    {
        builder.ToTable("order_sequence");
        builder.HasKey(os => new { os.AccountId, os.DrinkName });
        builder.Property(os => os.AccountId).HasColumnName("account_id").HasMaxLength(20);
        builder.Property(os => os.DrinkName).HasColumnName("drink_name").HasMaxLength(100);
        builder.Property(os => os.SequenceCount).HasColumnName("sequence_count").HasDefaultValue(0);

        builder.HasOne(os => os.Account).WithMany(a => a.OrderSequences);
    }
}
