using Microsoft.EntityFrameworkCore;
using BAR.Database.Configurations;
using BAR.Database.Entities;

namespace BAR.Database;

public class BarDbContext : DbContext
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Balance> Balances => Set<Balance>();
    public DbSet<MoodTracking> MoodTrackings => Set<MoodTracking>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Drink> Drinks => Set<Drink>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<DrinkIngredient> DrinkIngredients => Set<DrinkIngredient>();
    public DbSet<OrderSequence> OrderSequences => Set<OrderSequence>();
    public DbSet<RateLimit> RateLimits => Set<RateLimit>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    public DbSet<AccountPromo> AccountPromos => Set<AccountPromo>();
    public DbSet<PromoSetting> PromoSettings => Set<PromoSetting>();

    public BarDbContext(DbContextOptions<BarDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new ProfileConfiguration());
        modelBuilder.ApplyConfiguration(new BalanceConfiguration());
        modelBuilder.ApplyConfiguration(new MoodTrackingConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new DrinkConfiguration());
        modelBuilder.ApplyConfiguration(new IngredientConfiguration());
        modelBuilder.ApplyConfiguration(new DrinkIngredientConfiguration());
        modelBuilder.ApplyConfiguration(new OrderSequenceConfiguration());
        modelBuilder.ApplyConfiguration(new RateLimitConfiguration());
        modelBuilder.ApplyConfiguration(new PromoCodeConfiguration());
        modelBuilder.ApplyConfiguration(new AccountPromoConfiguration());
        modelBuilder.ApplyConfiguration(new PromoSettingConfiguration());

        SeedData(modelBuilder);
    }

    // Для dotnet ef migrations и design-time
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=bar_db;Username=postgres;Password=postgres");
        }
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ingredient>().HasData(
            new Ingredient { Id = 1, Name = "водка" },
            new Ingredient { Id = 2, Name = "ром" },
            new Ingredient { Id = 3, Name = "текила" },
            new Ingredient { Id = 4, Name = "виски" },
            new Ingredient { Id = 5, Name = "джин" },
            new Ingredient { Id = 6, Name = "кола" },
            new Ingredient { Id = 7, Name = "сок" },
            new Ingredient { Id = 8, Name = "тоник" },
            new Ingredient { Id = 9, Name = "лёд" },
            new Ingredient { Id = 10, Name = "молоко" }
        );

        modelBuilder.Entity<Drink>().HasData(
            // Дневные напитки
            new Drink { Id = 1, Name = "Куба Либре", BasePrice = 15, IsNight = false, IsHidden = false },
            new Drink { Id = 2, Name = "Отвёртка", BasePrice = 12, IsNight = false, IsHidden = false },
            new Drink { Id = 3, Name = "Джин-тоник", BasePrice = 14, IsNight = false, IsHidden = false },
            new Drink { Id = 4, Name = "Виски-кола", BasePrice = 13, IsNight = false, IsHidden = false },
            new Drink { Id = 5, Name = "Текила-санрайз", BasePrice = 14, IsNight = false, IsHidden = false },
            new Drink { Id = 6, Name = "Русский", BasePrice = 10, IsNight = false, IsHidden = false },
            new Drink { Id = 7, Name = "Белый русский", BasePrice = 16, IsNight = false, IsHidden = false },
            new Drink { Id = 8, Name = "Лонг-Айленд", BasePrice = 25, IsNight = false, IsHidden = false },
            // Ночные напитки
            new Drink { Id = 9, Name = "Ночной русский", BasePrice = 8, IsNight = true, IsHidden = false },
            new Drink { Id = 10, Name = "Бессонница", BasePrice = 10, IsNight = true, IsHidden = false },
            new Drink { Id = 11, Name = "Лунный свет", BasePrice = 12, IsNight = true, IsHidden = false },
            // Мертвец: водка + ром + молоко (скрыт из меню, удваивает баланс)
            new Drink { Id = 12, Name = "Мертвец", BasePrice = 0, IsNight = false, IsHidden = true }
        );

        modelBuilder.Entity<DrinkIngredient>().HasData(
            // Куба Либре: ром + кола + лёд
            new DrinkIngredient { DrinkId = 1, IngredientId = 2 },
            new DrinkIngredient { DrinkId = 1, IngredientId = 6 },
            new DrinkIngredient { DrinkId = 1, IngredientId = 9 },
            // Отвёртка: водка + сок
            new DrinkIngredient { DrinkId = 2, IngredientId = 1 },
            new DrinkIngredient { DrinkId = 2, IngredientId = 7 },
            // Джин-тоник: джин + тоник + лёд
            new DrinkIngredient { DrinkId = 3, IngredientId = 5 },
            new DrinkIngredient { DrinkId = 3, IngredientId = 8 },
            new DrinkIngredient { DrinkId = 3, IngredientId = 9 },
            // Виски-кола: виски + кола
            new DrinkIngredient { DrinkId = 4, IngredientId = 4 },
            new DrinkIngredient { DrinkId = 4, IngredientId = 6 },
            // Текила-санрайз: текила + сок
            new DrinkIngredient { DrinkId = 5, IngredientId = 3 },
            new DrinkIngredient { DrinkId = 5, IngredientId = 7 },
            // Русский: водка + лёд
            new DrinkIngredient { DrinkId = 6, IngredientId = 1 },
            new DrinkIngredient { DrinkId = 6, IngredientId = 9 },
            // Белый русский: водка + лёд + молоко
            new DrinkIngredient { DrinkId = 7, IngredientId = 1 },
            new DrinkIngredient { DrinkId = 7, IngredientId = 9 },
            new DrinkIngredient { DrinkId = 7, IngredientId = 10 },
            // Лонг-Айленд: водка + ром + текила + джин + кола
            new DrinkIngredient { DrinkId = 8, IngredientId = 1 },
            new DrinkIngredient { DrinkId = 8, IngredientId = 2 },
            new DrinkIngredient { DrinkId = 8, IngredientId = 3 },
            new DrinkIngredient { DrinkId = 8, IngredientId = 5 },
            new DrinkIngredient { DrinkId = 8, IngredientId = 6 },
            // Ночной русский: водка + лёд + молоко
            new DrinkIngredient { DrinkId = 9, IngredientId = 1 },
            new DrinkIngredient { DrinkId = 9, IngredientId = 9 },
            new DrinkIngredient { DrinkId = 9, IngredientId = 10 },
            // Бессонница: ром + кола + тоник
            new DrinkIngredient { DrinkId = 10, IngredientId = 2 },
            new DrinkIngredient { DrinkId = 10, IngredientId = 6 },
            new DrinkIngredient { DrinkId = 10, IngredientId = 8 },
            // Лунный свет: джин + сок + тоник
            new DrinkIngredient { DrinkId = 11, IngredientId = 5 },
            new DrinkIngredient { DrinkId = 11, IngredientId = 7 },
            new DrinkIngredient { DrinkId = 11, IngredientId = 8 },
            // Мертвец: водка + ром + молоко
            new DrinkIngredient { DrinkId = 12, IngredientId = 1 },
            new DrinkIngredient { DrinkId = 12, IngredientId = 2 },
            new DrinkIngredient { DrinkId = 12, IngredientId = 10 }
            );
        // Промокоды
        modelBuilder.Entity<PromoCode>().HasData(
            new PromoCode { Id = 1, Code = "ANTIHACK", MaxUses = null, UsesLeft = 999999, BonusBalance = 50 },
            new PromoCode { Id = 2, Code = "FREESHOT", MaxUses = 3, UsesLeft = 3, FreeDrink = true },
            new PromoCode { Id = 3, Code = "RICHBOY", MaxUses = null, UsesLeft = 999999, BonusBalance = 500 },
            new PromoCode { Id = 4, Code = "GOODMOOD", MaxUses = null, UsesLeft = 999999, MoodEffect = "friendly" },
            new PromoCode { Id = 5, Code = "NIGHT", MaxUses = null, UsesLeft = 999999, NightAccess = true },
            new PromoCode { Id = 6, Code = "LEGEND", MaxUses = 1, UsesLeft = 1, BonusBalance = 10000 }
        );

         // Настройка промо-системы
         modelBuilder.Entity<PromoSetting>().HasData(
             new PromoSetting { Id = 1, PromoEnabled = true }  // Включаем по умолчанию
         );
    }
}