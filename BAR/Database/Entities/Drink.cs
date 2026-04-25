namespace BAR.Database.Entities;

public class Drink
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int BasePrice { get; set; }
    public bool IsNight { get; set; } = false;
    public bool IsHidden { get; set; } = false;
    public int MoodPriceModifier { get; set; } = 0;
    public int MinDrinkCount { get; set; } = 0;

    public ICollection<DrinkIngredient> DrinkIngredients { get; set; } = new List<DrinkIngredient>();
}
