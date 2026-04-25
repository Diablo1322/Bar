namespace BAR.Database.Entities;

public class DrinkIngredient
{
    public int DrinkId { get; set; }
    public int IngredientId { get; set; }

    public Drink? Drink { get; set; }
    public Ingredient? Ingredient { get; set; }
}
