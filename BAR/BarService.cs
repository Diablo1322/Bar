using BAR.Database;
using BAR.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace BAR.Services;

public interface IBarService
{
    Task<IResult> RegisterAsync();
    Task<IResult> ResetAsync(string token);
    Task<IResult> GetMenuAsync(string token, string xTime);
    Task<IResult> OrderDrinkAsync(string token, string drinkName, string xTime);
    Task<IResult> MixDrinkAsync(string token, List<string> ingredients, string xTime);
    Task<IResult> GetBalanceAsync(string token);
    Task<IResult> TipAsync(string token, int amount);
    Task<IResult> GetHistoryAsync(string token);
    Task<IResult> GetProfileAsync(string token);
    Task<IResult> ActivatePromoAsync(string token, string code);
    Task<IResult> GetActivePromosAsync(string token);
}

public class BarService : IBarService
{
    private readonly BarDbContext _db;

    public BarService(BarDbContext db) => _db = db;

    public async Task<IResult> RegisterAsync()
    {
        var token = Guid.NewGuid().ToString("n");
        var id = $"BAR-{Random.Shared.Next(1000, 9999):D4}";

        var account = new Account { Id = id, Token = token, CreatedAt = DateTime.UtcNow };

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();

        _db.Balances.Add(new Balance { AccountId = id, Amount = 100 });
        _db.Profiles.Add(new Profile { AccountId = id });
        _db.MoodTrackings.Add(new MoodTracking { AccountId = id });
        _db.RateLimits.Add(new RateLimit { AccountId = id });

        await _db.SaveChangesAsync();

        return Results.Ok(new { status = "ok", id, token });
    }

    public async Task<IResult> ResetAsync(string token)
    {
        var account = await GetAccountWithAll(token);
        if (account == null) return Results.Unauthorized();

        account.Balance!.Amount = 100;
        account.Profile!.Rank = "Новичок";
        account.Profile.TotalOrders = 0;
        account.Profile.UniqueDrinks = 0;
        account.Profile.FavoriteDrink = null;
        account.Profile.BarClosed = false;
        account.Profile.HasNightAccess = false;

        account.MoodTracking!.MoodLevel = "normal";
        account.MoodTracking.ConsecutiveSimilarOrders = 0;
        account.MoodTracking.LastDrink = null;
        account.MoodTracking.LastOrderTime = null;

        account.RateLimit!.RequestCount = 0;
        account.RateLimit.WindowStart = DateTime.UtcNow;

        _db.Orders.RemoveRange(account.Orders);
        _db.OrderSequences.RemoveRange(account.OrderSequences);

        await _db.SaveChangesAsync();
        return Results.Ok(new { status = "ok" });
    }

    private async Task<(bool allowed, int? retryAfter)> CheckRateLimit(Account account)
    {
        var rl = account.RateLimit!;
        var now = DateTime.UtcNow;

        if ((now - rl.WindowStart).TotalMinutes >= 1)
        {
            rl.RequestCount = 0;
            rl.WindowStart = now;
        }

        rl.RequestCount++;
        await _db.SaveChangesAsync();

        if (rl.RequestCount > 15)
        {
            var retryAfter = (int)(60 - (now - rl.WindowStart).TotalSeconds);
            return (false, Math.Max(1, retryAfter));
        }

        return (true, null);
    }

    public async Task<IResult> GetMenuAsync(string token, string xTime)
    {
        var account = await GetAccountWithAll(token);
        if (account == null) return Results.Unauthorized();

        var (allowed, retryAfter) = await CheckRateLimit(account);
        if (!allowed) return Results.Ok(new { status = "error", error = "rate_limit", retry_after = retryAfter });

        if (account.Profile!.BarClosed)
            return Results.Ok(new { status = "error", error = "bar_closed" });

        // НОВОЕ: NIGHT промокод переопределяет время суток
        bool isNight = account.Profile.HasNightAccess || IsNightTime(xTime);

        var totalOrders = account.Profile.TotalOrders;

        var drinks = await _db.Drinks
            .Include(d => d.DrinkIngredients)
            .ThenInclude(di => di.Ingredient)
            .Where(d => !d.IsHidden)
            .ToListAsync();

        var drinkList = drinks
            .Where(d => d.MinDrinkCount <= totalOrders)
            .Where(d => isNight ? d.IsNight : !d.IsNight)
            .Select(d => new
            {
                name = d.Name,
                price = CalculatePrice(d, account.MoodTracking!.MoodLevel),
                ingredients = d.DrinkIngredients.Select(di => di.Ingredient!.Name).ToList()
            })
            .ToList();

        return Results.Ok(new
        {
            status = "ok",
            drinks = drinkList,
            balance = account.Balance!.Amount,
            mood_level = account.MoodTracking!.MoodLevel
        });
    }

    public async Task<IResult> OrderDrinkAsync(string token, string drinkName, string xTime)
    {
        var account = await GetAccountWithAll(token);
        if (account == null) return Results.Unauthorized();

        var (allowed, retryAfter) = await CheckRateLimit(account);
        if (!allowed) return Results.Ok(new { status = "error", error = "rate_limit", retry_after = retryAfter });

        if (account.Profile!.BarClosed) return Results.Ok(new { status = "error", error = "bar_closed" });

        var drink = await _db.Drinks.FirstOrDefaultAsync(d => d.Name == drinkName);
        if (drink == null || drink.IsHidden || drink.MinDrinkCount > account.Profile.TotalOrders)
            return Results.Ok(new { status = "error", error = "unknown_drink", balance = account.Balance!.Amount, mood_level = account.MoodTracking!.MoodLevel });

        var consecutive = await GetConsecutiveCount(account.Id, drinkName);
        var nextConsecutive = consecutive + 1;

        var price = CalculatePrice(drink, account.MoodTracking!.MoodLevel);

        if (nextConsecutive == 8)
            price = 0;

        if (account.Balance!.Amount < price && price != 0)
            return Results.Ok(new { status = "error", error = "insufficient_funds", price, balance = account.Balance.Amount, mood_level = account.MoodTracking.MoodLevel });

        account.Balance.Amount -= price;

        _db.Orders.Add(new Order
        {
            AccountId = account.Id,
            Drink = drinkName,
            Price = price,
            Method = "order",
            CreatedAt = DateTime.UtcNow
        });

        await UpdateMoodAndProfile(account, drinkName, "order");
        await _db.SaveChangesAsync();

        return Results.Ok(new
        {
            status = "ok",
            drink = drinkName,
            price,
            balance = account.Balance.Amount,
            mood_level = account.MoodTracking!.MoodLevel
        });
    }

    public async Task<IResult> MixDrinkAsync(string token, List<string> ingredients, string xTime)
    {
        var account = await GetAccountWithAll(token);
        if (account == null) return Results.Unauthorized();

        var (allowed, retryAfter) = await CheckRateLimit(account);
        if (!allowed)
            return Results.Ok(new { status = "error", error = "rate_limit", retry_after = retryAfter });

        if (account.Profile!.BarClosed)
            return Results.Ok(new { status = "error", error = "bar_closed" });

        if (ingredients == null || ingredients.Count == 0)
            return Results.Ok(new
            {
                status = "error",
                error = "unknown_recipe",
                balance = account.Balance!.Amount,
                mood_level = account.MoodTracking!.MoodLevel
            });

        var normalized = ingredients.Select(i => i.ToLowerInvariant().Trim()).OrderBy(x => x).ToList();

        bool isNight = account.Profile.HasNightAccess || IsNightTime(xTime);

        var drinks = await _db.Drinks
            .Include(d => d.DrinkIngredients)
            .ThenInclude(di => di.Ingredient)
            .Where(d => d.IsHidden || (isNight ? d.IsNight : !d.IsNight))
            .ToListAsync();

        Drink? matchingDrink = null;
        foreach (var d in drinks)
        {
            var drinkIngs = d.DrinkIngredients
                .Select(di => di.Ingredient!.Name.ToLowerInvariant().Trim())
                .OrderBy(x => x)
                .ToList();

            if (normalized.SequenceEqual(drinkIngs))
            {
                matchingDrink = d;
                break;
            }
        }

        if (matchingDrink == null)
        {
            account.MoodTracking!.MoodLevel = account.MoodTracking.MoodLevel switch
            {
                "normal" => "grumpy",
                "grumpy" => "hostile",
                "hostile" => "hostile",
                _ => account.MoodTracking.MoodLevel
            };
            await _db.SaveChangesAsync();

            return Results.Ok(new
            {
                status = "error",
                error = "unknown_recipe",
                balance = account.Balance!.Amount,
                mood_level = account.MoodTracking!.MoodLevel
            });
        }

        var consecutive = await GetConsecutiveCount(account.Id, matchingDrink.Name);
        var nextConsecutive = consecutive + 1;

        var basePrice = CalculatePrice(matchingDrink, account.MoodTracking!.MoodLevel);
        var price = (int)Math.Max(5, basePrice * 0.8);

        bool isFreeEvery7th = false;
        if (nextConsecutive == 7)
        {
            price = 0;
            isFreeEvery7th = true;
        }

        if (matchingDrink.IsHidden)
            price = 0;

        if (account.Balance!.Amount < price && price != 0)
            return Results.Ok(new
            {
                status = "error",
                error = "insufficient_funds",
                price = price,
                balance = account.Balance.Amount,
                mood_level = account.MoodTracking.MoodLevel
            });

        account.Balance.Amount -= price;

        if (matchingDrink.Name == "Мертвец")
            account.Balance.Amount = account.Balance.Amount * 2;

        _db.Orders.Add(new Order
        {
            AccountId = account.Id,
            Drink = matchingDrink.Name,
            Price = price,
            Method = "mix",
            CreatedAt = DateTime.UtcNow
        });

        await UpdateMoodAndProfile(account, matchingDrink.Name, "mix");
        await _db.SaveChangesAsync();

        var result = new Dictionary<string, object>
        {
            ["status"] = "ok",
            ["drink"] = matchingDrink.Name,
            ["price"] = price,
            ["balance"] = account.Balance.Amount,
            ["mood_level"] = account.MoodTracking!.MoodLevel
        };

        if (isFreeEvery7th)
            result["free_every_7th"] = true;

        if (matchingDrink.IsHidden)
        {
            result["secret"] = true;
            if (matchingDrink.Name == "Мертвец")
                result["effect"] = "balance_doubled";
        }

        return Results.Ok(result);
    }

    public async Task<IResult> GetBalanceAsync(string token)
    {
        var account = await GetAccountWithAll(token);
        if (account == null) return Results.Unauthorized();

        var (allowed, retryAfter) = await CheckRateLimit(account);
        if (!allowed) return Results.Ok(new { status = "error", error = "rate_limit", retry_after = retryAfter });

        return Results.Ok(new
        {
            status = "ok",
            balance = account.Balance!.Amount,
            mood_level = account.MoodTracking!.MoodLevel,
        });
    }

    public async Task<IResult> TipAsync(string token, int amount)
    {
        var account = await GetAccountWithAll(token);
        if (account == null) return Results.Unauthorized();

        var (allowed, retryAfter) = await CheckRateLimit(account);
        if (!allowed) return Results.Ok(new { status = "error", error = "rate_limit", retry_after = retryAfter });

        if (amount <= 0)
            return Results.Ok(new { status = "error", error = "invalid_amount", balance = account.Balance!.Amount, mood_level = account.MoodTracking!.MoodLevel });

        if (account.Balance!.Amount < amount)
            return Results.Ok(new { status = "error", error = "insufficient_funds", balance = account.Balance.Amount, mood_level = account.MoodTracking!.MoodLevel });

        account.Balance.Amount -= amount;

        if (amount >= 10)
            account.MoodTracking!.MoodLevel = "friendly";
        else
            account.MoodTracking!.MoodLevel = "normal";

        await _db.SaveChangesAsync();

        return Results.Ok(new
        {
            status = "ok",
            tip = amount,
            balance = account.Balance.Amount,
            mood_level = account.MoodTracking!.MoodLevel
        });
    }

    public async Task<IResult> GetHistoryAsync(string token)
    {
        var account = await GetAccountWithAll(token);
        if (account == null) return Results.Unauthorized();

        var (allowed, retryAfter) = await CheckRateLimit(account);
        if (!allowed) return Results.Ok(new { status = "error", error = "rate_limit", retry_after = retryAfter });

        var orders = account.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new { drink = o.Drink, price = o.Price, method = o.Method })
            .ToList();

        return Results.Ok(new
        {
            status = "ok",
            orders,
            balance = account.Balance!.Amount,
            mood_level = account.MoodTracking!.MoodLevel
        });
    }

    public async Task<IResult> GetProfileAsync(string token)
    {
        var account = await GetAccountWithAll(token);
        if (account == null) return Results.Unauthorized();

        var (allowed, retryAfter) = await CheckRateLimit(account);
        if (!allowed) return Results.Ok(new { status = "error", error = "rate_limit", retry_after = retryAfter });

        return Results.Ok(new
        {
            status = "ok",
            id = account.Id,
            rank = account.Profile!.Rank,
            total_orders = account.Profile.TotalOrders,
            unique_drinks = account.Profile.UniqueDrinks,
            favorite_drink = account.Profile.FavoriteDrink,
            bar_closed = account.Profile.BarClosed,
            has_night_access = account.Profile.HasNightAccess
        });
    }

    // ====================== HELPERS ======================
    private async Task<Account?> GetAccountWithAll(string token)
    {
        return await _db.Accounts
            .Include(a => a.Balance)
            .Include(a => a.Profile)
            .Include(a => a.MoodTracking)
            .Include(a => a.Orders)
            .Include(a => a.OrderSequences)
            .Include(a => a.RateLimit)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Token == token);
    }

    private async Task<int> GetConsecutiveCount(string accountId, string drinkName)
    {
        var seq = await _db.OrderSequences
            .FirstOrDefaultAsync(os => os.AccountId == accountId && os.DrinkName == drinkName);
        return seq?.SequenceCount ?? 0;
    }

    private bool IsNightTime(string xTime)
    {
        if (string.IsNullOrEmpty(xTime) || !TimeOnly.TryParse(xTime, out var time))
            return false; // по умолчанию день

        return time.Hour >= 0 && time.Hour < 6;
    }

    private int CalculatePrice(Drink drink, string moodLevel)
    {
        int price = drink.BasePrice + drink.MoodPriceModifier;

        switch (moodLevel)
        {
            case "grumpy":
                if (drink.Name == "Русский")
                    price = Math.Max(5, price - 1);
                else
                    price += 3;
                break;
            case "hostile":
                if (drink.Name == "Русский")
                    price = Math.Max(5, price + 2);
                else
                    price += 6;
                break;
            case "friendly":
                price = Math.Max(5, price - 2);
                break;
        }

        return Math.Max(5, price);
    }

    private async Task UpdateMoodAndProfile(Account account, string drinkName, string method)
    {
        var mood = account.MoodTracking!;
        var profile = account.Profile!;

        var sequence = await _db.OrderSequences
            .FirstOrDefaultAsync(os => os.AccountId == account.Id && os.DrinkName == drinkName);

        if (sequence == null)
        {
            sequence = new OrderSequence { AccountId = account.Id, DrinkName = drinkName, SequenceCount = 1 };
            _db.OrderSequences.Add(sequence);
        }
        else
        {
            sequence.SequenceCount++;
        }

        mood.ConsecutiveSimilarOrders = sequence.SequenceCount;
        mood.LastDrink = drinkName;
        mood.LastOrderTime = DateTime.UtcNow;

        // Mix всегда даёт friendly
        if (method == "mix")
        {
            mood.MoodLevel = "friendly";
        }
        else if (mood.ConsecutiveSimilarOrders >= 8)
        {
            mood.MoodLevel = "hostile";
        }
        else if (mood.ConsecutiveSimilarOrders >= 3)
        {
            mood.MoodLevel = "grumpy";
        }
        else
        {
            mood.MoodLevel = "normal";
        }

        profile.TotalOrders++;

        profile.UniqueDrinks = await _db.Orders
            .Where(o => o.AccountId == account.Id)
            .Select(o => o.Drink)
            .Distinct()
            .CountAsync();

        profile.FavoriteDrink = await _db.Orders
            .Where(o => o.AccountId == account.Id)
            .GroupBy(o => o.Drink)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync();

        if (profile.TotalOrders >= 50)
            profile.Rank = "Профи";
        else if (profile.TotalOrders >= 6)
            profile.Rank = "Постоянный";
        else
            profile.Rank = "Новичок";

        await _db.SaveChangesAsync();
    }

    public async Task<IResult> ActivatePromoAsync(string token, string code)
    {
        var account = await GetAccountWithAllForPromo(token);
        if (account == null) return Results.Unauthorized();

        var (allowed, retryAfter) = await CheckRateLimit(account);
        if (!allowed) return Results.Ok(new { status = "error", error = "rate_limit", retry_after = retryAfter });

        // Проверяем, включена ли промо-система
        var setting = await _db.PromoSettings.FirstAsync();
        if (!setting.PromoEnabled)
            return Results.NotFound();

        if (string.IsNullOrEmpty(code))
            return Results.Ok(new { status = "error", error = "invalid_code", balance = account.Balance!.Amount, mood_level = account.MoodTracking!.MoodLevel });

        var promo = await _db.PromoCodes.FirstOrDefaultAsync(p => p.Code == code && p.IsEnabled);
        if (promo == null)
            return Results.Ok(new { status = "error", error = "invalid_code", balance = account.Balance!.Amount, mood_level = account.MoodTracking!.MoodLevel });

        // Проверяем, не использовал ли уже этот код
        var alreadyUsed = await _db.AccountPromos.AnyAsync(ap => ap.AccountId == account.Id && ap.Code == code);
        if (alreadyUsed)
            return Results.Ok(new { status = "error", error = "already_used", balance = account.Balance!.Amount, mood_level = account.MoodTracking!.MoodLevel });

        // Проверяем лимит использований
        if (promo.UsesLeft <= 0)
            return Results.Ok(new { status = "error", error = "invalid_code", balance = account.Balance!.Amount, mood_level = account.MoodTracking!.MoodLevel });

        // ==================== ПРИМЕНЯЕМ ЭФФЕКТЫ (один раз!) ====================

        // Бонус к балансу (ANTIHACK, RICHBOY, LEGEND и т.д.)
        if (promo.BonusBalance > 0)
        {
            account.Balance!.Amount += promo.BonusBalance;
        }

        // Изменение настроения (GOODMOOD)
        if (!string.IsNullOrEmpty(promo.MoodEffect))
        {
            account.MoodTracking!.MoodLevel = promo.MoodEffect;
        }

        // Бесплатный напиток (FREESHOT)
        if (promo.FreeDrink)
        {
            account.Balance!.Amount += 10;   // эквивалент "Русский"
        }

        // Ночной доступ (NIGHT)
        if (promo.NightAccess)
        {
            account.Profile!.HasNightAccess = true;
        }

        // Уменьшаем счётчик использований
        promo.UsesLeft--;

        // Записываем использование промокода
        _db.AccountPromos.Add(new AccountPromo { AccountId = account.Id, Code = code });

        await _db.SaveChangesAsync();

        return Results.Ok(new
        {
            status = "ok",
            code,
            balance = account.Balance!.Amount,
            mood_level = account.MoodTracking!.MoodLevel
        });
    }

    public async Task<IResult> GetActivePromosAsync(string token)
    {
        var account = await GetAccountWithAllForPromo(token);
        if (account == null) return Results.Unauthorized();

        var (allowed, retryAfter) = await CheckRateLimit(account);
        if (!allowed) return Results.Ok(new { status = "error", error = "rate_limit", retry_after = retryAfter });

        // Проверяем, включена ли промо-система
        var setting = await _db.PromoSettings.FirstAsync();
        if (!setting.PromoEnabled)
            return Results.NotFound();

        var active = await _db.PromoCodes
            .Where(p => p.IsEnabled && p.UsesLeft > 0)
            .Select(p => new
            {
                code = p.Code,
                remaining = p.MaxUses == null ? (int?)null : p.UsesLeft
            })
            .ToListAsync();

        return Results.Ok(new
        {
            status = "ok",
            active,
            balance = account.Balance!.Amount,
            mood_level = account.MoodTracking!.MoodLevel
        });
    }

    private async Task<Account?> GetAccountWithAllForPromo(string token)
    {
        return await _db.Accounts
            .Include(a => a.Balance)
            .Include(a => a.Profile)
            .Include(a => a.MoodTracking)
            .Include(a => a.RateLimit)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Token == token);
    }

}