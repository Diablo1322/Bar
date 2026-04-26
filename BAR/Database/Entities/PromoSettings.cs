namespace BAR.Database.Entities;

public class PromoSetting
{
    public int Id { get; set; }
    public bool PromoEnabled { get; set; } = true;  // по умолчанию ВКЛЮЧЕНА
}