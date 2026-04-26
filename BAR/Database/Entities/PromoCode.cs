namespace BAR.Database.Entities;

public class PromoCode
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int? MaxUses { get; set; }  // null = безлимитный
    public int UsesLeft { get; set; }  // оставшиеся использования
    public int BonusBalance { get; set; } = 0;
    public string? MoodEffect { get; set; }  // "friendly", "generous" и т.д.
    public bool NightAccess { get; set; } = false;  // доступ к ночному меню
    public bool FreeDrink { get; set; } = false;  // бесплатный напиток
    public bool IsEnabled { get; set; } = true;
}