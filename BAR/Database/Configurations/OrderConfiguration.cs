using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BAR.Database.Entities;

namespace BAR.Database.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id").UseIdentityByDefaultColumn();
        builder.Property(o => o.AccountId).HasColumnName("account_id").HasMaxLength(20).IsRequired();
        builder.Property(o => o.Drink).HasColumnName("drink").HasMaxLength(100).IsRequired();
        builder.Property(o => o.Price).HasColumnName("price").IsRequired();
        builder.Property(o => o.Method).HasColumnName("method").HasMaxLength(20).IsRequired();
        builder.Property(o => o.CreatedAt).HasColumnName("created_at");

        builder.HasOne(o => o.Account).WithMany(a => a.Orders);
    }
}
