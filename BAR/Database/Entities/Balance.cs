namespace BAR.Database.Entities;

public class Balance
{
    public string AccountId { get; set; } = string.Empty;
    public long Amount { get; set; } = 100;

    public Account? Account { get; set; }
}
