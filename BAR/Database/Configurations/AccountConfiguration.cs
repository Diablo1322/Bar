using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BAR.Database.Entities;

namespace BAR.Database.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id").HasMaxLength(20);
        builder.Property(a => a.Token).HasColumnName("token").HasMaxLength(64).IsRequired();
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(a => a.Token).IsUnique();
    }
}
