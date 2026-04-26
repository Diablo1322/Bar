namespace BAR.Database.Entities;

public class AccountPromo
{
    public int Id { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    public Account? Account { get; set; }
}