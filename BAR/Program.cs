using BAR.Database;
using BAR.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Настройка JSON — чтобы поля были в нижнем регистре как в ТЗ
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddDbContext<BarDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IBarService, BarService>();

var app = builder.Build();

// Применяем миграции при старте
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BarDbContext>();
    db.Database.Migrate();
}

// ====================== ОБРАБОТКА НЕИЗВЕСТНЫХ ЭНДПОИНТОВ ======================
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    if (response.StatusCode == 404)
    {
        response.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync("{\"status\":\"error\",\"error\":\"not_found\"}");
    }
});

// ====================== ЭНДПОИНТЫ ======================

app.MapPost("/register", async (IBarService service) => await service.RegisterAsync());

app.MapPost("/reset", async (HttpContext ctx, IBarService service) =>
{
    if (!TryGetToken(ctx, out var token)) return Results.Unauthorized();
    return await service.ResetAsync(token);
});

app.MapGet("/menu", async (HttpContext ctx, IBarService service) =>
{
    if (!TryGetToken(ctx, out var token)) return Results.Unauthorized();
    var xTime = ctx.Request.Headers["X-Time"].ToString();
    return await service.GetMenuAsync(token, xTime);
});

app.MapPost("/order", async (HttpContext ctx, OrderRequest req, IBarService service) =>
{
    if (!TryGetToken(ctx, out var token)) return Results.Unauthorized();
    var xTime = ctx.Request.Headers["X-Time"].ToString();
    return await service.OrderDrinkAsync(token, req.Name, xTime);
});

app.MapPost("/mix", async (HttpContext ctx, MixRequest req, IBarService service) =>
{
    if (!TryGetToken(ctx, out var token)) return Results.Unauthorized();
    var xTime = ctx.Request.Headers["X-Time"].ToString();
    return await service.MixDrinkAsync(token, req.Ingredients ?? new List<string>(), xTime);
});

app.MapGet("/balance", async (HttpContext ctx, IBarService service) =>
{
    if (!TryGetToken(ctx, out var token)) return Results.Unauthorized();
    return await service.GetBalanceAsync(token);
});

app.MapPost("/tip", async (HttpContext ctx, TipRequest req, IBarService service) =>
{
    if (!TryGetToken(ctx, out var token)) return Results.Unauthorized();
    return await service.TipAsync(token, req.Amount);
});

app.MapGet("/history", async (HttpContext ctx, IBarService service) =>
{
    if (!TryGetToken(ctx, out var token)) return Results.Unauthorized();
    return await service.GetHistoryAsync(token);
});

app.MapGet("/profile", async (HttpContext ctx, IBarService service) =>
{
    if (!TryGetToken(ctx, out var token)) return Results.Unauthorized();
    return await service.GetProfileAsync(token);
});

// ====================== ПРОМОКОДЫ ======================

app.MapPost("/promo", async (HttpContext ctx, PromoRequest req, IBarService service) =>
{
    if (!TryGetToken(ctx, out var token)) return Results.Unauthorized();
    return await service.ActivatePromoAsync(token, req.Code);
});

app.MapGet("/promo", async (HttpContext ctx, IBarService service) =>
{
    if (!TryGetToken(ctx, out var token)) return Results.Unauthorized();
    return await service.GetActivePromosAsync(token);
});

// ====================== АДМИНИСТРИРОВАНИЕ ПРОМО ======================
var adminKey = Environment.GetEnvironmentVariable("ADMIN_KEY") ?? "AntiHack2026Secret";

app.MapPost("/admin/promo", async (HttpContext ctx, AdminPromoRequest req) =>
{
    if (req.Key != adminKey)
        return Results.Forbid();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BarDbContext>();
    var setting = await db.PromoSettings.FirstAsync();
    setting.PromoEnabled = req.Enabled;
    await db.SaveChangesAsync();

    return Results.Ok(new { status = "ok", promo_enabled = setting.PromoEnabled });
});

app.MapGet("/admin/promo", async () =>
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BarDbContext>();
    var setting = await db.PromoSettings.FirstAsync();
    return Results.Ok(new { promo_enabled = setting.PromoEnabled });
});

app.Run("http://0.0.0.0:8000");

bool TryGetToken(HttpContext ctx, out string token)
{
    token = string.Empty;
    var auth = ctx.Request.Headers.Authorization.ToString();
    if (string.IsNullOrEmpty(auth) || !auth.StartsWith("Bearer ")) return false;
    token = auth["Bearer ".Length..].Trim();
    return !string.IsNullOrEmpty(token);
}

public record OrderRequest(string Name);
public record MixRequest(List<string> Ingredients);
public record TipRequest(int Amount);
public record PromoRequest(string Code);
public record AdminPromoRequest(bool Enabled, string Key);