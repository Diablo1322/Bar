namespace BAR.Database.Entities;

public class RateLimit
{
    public string AccountId { get; set; } = string.Empty;
    public int RequestCount { get; set; } = 0;
    public DateTime WindowStart { get; set; } = DateTime.UtcNow;

    public Account? Account { get; set; }
}
