namespace BAR.Database.Entities;

public class OrderSequence
{
    public string AccountId { get; set; } = string.Empty;
    public string DrinkName { get; set; } = string.Empty;
    public int SequenceCount { get; set; } = 0;

    public Account? Account { get; set; }
}
