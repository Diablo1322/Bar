using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BAR.Database.Entities;

namespace BAR.Database.Configurations;

public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.ToTable("ingredients");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id").UseIdentityByDefaultColumn();
        builder.Property(i => i.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        builder.HasIndex(i => i.Name).IsUnique();
    }
}
