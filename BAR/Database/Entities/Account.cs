namespace BAR.Database.Entities;

public class Account
{
    public string Id { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Profile? Profile { get; set; }
    public Balance? Balance { get; set; }
    public MoodTracking? MoodTracking { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<OrderSequence> OrderSequences { get; set; } = new List<OrderSequence>();
    public RateLimit? RateLimit { get; set; }
}
