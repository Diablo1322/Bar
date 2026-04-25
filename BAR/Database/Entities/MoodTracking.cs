namespace BAR.Database.Entities;

public class MoodTracking
{
    public string AccountId { get; set; } = string.Empty;
    public string MoodLevel { get; set; } = "normal";
    public int ConsecutiveSimilarOrders { get; set; } = 0;
    public string? LastDrink { get; set; }
    public DateTime? LastOrderTime { get; set; }

    public Account? Account { get; set; }
}
