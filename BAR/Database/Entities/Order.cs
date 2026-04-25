namespace BAR.Database.Entities;

public class Order
{
    public int Id { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public string Drink { get; set; } = string.Empty;
    public int Price { get; set; }
    public string Method { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Account? Account { get; set; }
}
