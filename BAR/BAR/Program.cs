using System.Collections.Concurrent;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
// Настройка JSON, чтобы поля были маленькими буквами (как в ТЗ)
builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();

// Хранилище данных в оперативной памяти
var db = new ConcurrentDictionary<string, UserData>();

// --- Эндпоинты ---

// 1. Регистрация
app.MapPost("/register", () => {
    var token = Guid.NewGuid().ToString("n"); // Чистый токен без тире
    var id = $"BAR-{Random.Shared.Next(1000, 9999)}";

    var newUser = new UserData { Id = id, Balance = 100, Mood = "normal" };
    db[token] = newUser;

    return Results.Ok(new { status = "ok", id = id, token = token });
});

// 2. Баланс (с проверкой токена)
app.MapGet("/balance", (HttpContext context) => {
    if (!TryGetUser(context, db, out var user)) return Results.Unauthorized();

    return Results.Ok(new
    {
        status = "ok",
        balance = user.Balance,
        mood_level = user.Mood
    });
});

// 3. Профиль
app.MapGet("/profile", (HttpContext context) => {
    if (!TryGetUser(context, db, out var user)) return Results.Unauthorized();

    return Results.Ok(new
    {
        status = "ok",
        id = user.Id,
        rank = "Новичок",
        total_orders = 0,
        unique_drinks = 0,
        favorite_drink = (string?)null,
        bar_closed = false
    });
});

// Запуск на порту 8000
app.Run("http://0.0.0.0:8000");

// --- Вспомогательные функции и модели ---

bool TryGetUser(HttpContext context, ConcurrentDictionary<string, UserData> database, out UserData user)
{
    user = null!;
    var authHeader = context.Request.Headers["Authorization"].ToString();
    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ")) return false;

    var token = authHeader.Replace("Bearer ", "");
    return database.TryGetValue(token, out user);
}

public class UserData
{
    public string Id { get; set; } = "";
    public int Balance { get; set; }
    public string Mood { get; set; } = "normal";
}