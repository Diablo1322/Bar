using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BAR.Database.Entities;

namespace BAR.Database.Configurations;

public class DrinkIngredientConfiguration : IEntityTypeConfiguration<DrinkIngredient>
{
    public void Configure(EntityTypeBuilder<DrinkIngredient> builder)
    {
        builder.ToTable("drink_ingredients");
        builder.HasKey(di => new { di.DrinkId, di.IngredientId });
        builder.Property(di => di.DrinkId).HasColumnName("drink_id");
        builder.Property(di => di.IngredientId).HasColumnName("ingredient_id");

        builder.HasOne(di => di.Drink)
            .WithMany(d => d.DrinkIngredients)
            .HasForeignKey(di => di.DrinkId);

        builder.HasOne(di => di.Ingredient)
            .WithMany(i => i.DrinkIngredients)
            .HasForeignKey(di => di.IngredientId);
    }
}
