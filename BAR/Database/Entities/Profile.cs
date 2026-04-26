namespace BAR.Database.Entities;

public class Profile
{
    public string AccountId { get; set; } = string.Empty;
    public string Rank { get; set; } = "Новичок";
    public int TotalOrders { get; set; } = 0;
    public int UniqueDrinks { get; set; } = 0;
    public string? FavoriteDrink { get; set; }
    public bool BarClosed { get; set; } = false;

    public bool HasNightAccess { get; set; } = false;

    public Account? Account { get; set; }
}
