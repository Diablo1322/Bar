namespace BAR.Database.Entities;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<DrinkIngredient> DrinkIngredients { get; set; } = new List<DrinkIngredient>();
}
