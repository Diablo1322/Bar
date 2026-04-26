using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BAR.Migrations
{
    /// <inheritdoc />
    public partial class ChangeBalanceToLong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "drinks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    base_price = table.Column<int>(type: "integer", nullable: false),
                    is_night = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    mood_price_modifier = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    min_drink_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drinks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ingredients",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingredients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "promo_codes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    max_uses = table.Column<int>(type: "integer", nullable: true),
                    uses_left = table.Column<int>(type: "integer", nullable: false),
                    bonus_balance = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    mood_effect = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    night_access = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    free_drink = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promo_codes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "promo_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    promo_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promo_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "account_promos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_promos", x => x.id);
                    table.ForeignKey(
                        name: "FK_account_promos_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "balances",
                columns: table => new
                {
                    account_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    balance = table.Column<long>(type: "bigint", nullable: false, defaultValue: 100L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_balances", x => x.account_id);
                    table.ForeignKey(
                        name: "FK_balances_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mood_tracking",
                columns: table => new
                {
                    account_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    mood_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "normal"),
                    consecutive_similar_orders = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_drink = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_order_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mood_tracking", x => x.account_id);
                    table.ForeignKey(
                        name: "FK_mood_tracking_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_sequence",
                columns: table => new
                {
                    account_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    drink_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sequence_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_sequence", x => new { x.account_id, x.drink_name });
                    table.ForeignKey(
                        name: "FK_order_sequence_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    drink = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false),
                    method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_orders_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    account_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    rank = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Новичок"),
                    total_orders = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    unique_drinks = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    favorite_drink = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bar_closed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    has_night_access = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    has_free_drink = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profiles", x => x.account_id);
                    table.ForeignKey(
                        name: "FK_profiles_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rate_limits",
                columns: table => new
                {
                    account_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    request_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    window_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rate_limits", x => x.account_id);
                    table.ForeignKey(
                        name: "FK_rate_limits_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "drink_ingredients",
                columns: table => new
                {
                    drink_id = table.Column<int>(type: "integer", nullable: false),
                    ingredient_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drink_ingredients", x => new { x.drink_id, x.ingredient_id });
                    table.ForeignKey(
                        name: "FK_drink_ingredients_drinks_drink_id",
                        column: x => x.drink_id,
                        principalTable: "drinks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_drink_ingredients_ingredients_ingredient_id",
                        column: x => x.ingredient_id,
                        principalTable: "ingredients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "drinks",
                columns: new[] { "id", "base_price", "name" },
                values: new object[,]
                {
                    { 1, 15, "Куба Либре" },
                    { 2, 12, "Отвёртка" },
                    { 3, 14, "Джин-тоник" },
                    { 4, 13, "Виски-кола" },
                    { 5, 14, "Текила-санрайз" },
                    { 6, 10, "Русский" },
                    { 7, 16, "Белый русский" },
                    { 8, 25, "Лонг-Айленд" }
                });

            migrationBuilder.InsertData(
                table: "drinks",
                columns: new[] { "id", "base_price", "is_night", "name" },
                values: new object[,]
                {
                    { 9, 8, true, "Ночной русский" },
                    { 10, 10, true, "Бессонница" },
                    { 11, 12, true, "Лунный свет" }
                });

            migrationBuilder.InsertData(
                table: "drinks",
                columns: new[] { "id", "base_price", "is_hidden", "name" },
                values: new object[] { 12, 0, true, "Мертвец" });

            migrationBuilder.InsertData(
                table: "ingredients",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "водка" },
                    { 2, "ром" },
                    { 3, "текила" },
                    { 4, "виски" },
                    { 5, "джин" },
                    { 6, "кола" },
                    { 7, "сок" },
                    { 8, "тоник" },
                    { 9, "лёд" },
                    { 10, "молоко" }
                });

            migrationBuilder.InsertData(
                table: "promo_codes",
                columns: new[] { "id", "code", "is_enabled", "max_uses", "mood_effect", "uses_left" },
                values: new object[] { 1, "ANTIHACK", true, 3, null, 3 });

            migrationBuilder.InsertData(
                table: "promo_codes",
                columns: new[] { "id", "code", "free_drink", "is_enabled", "max_uses", "mood_effect", "uses_left" },
                values: new object[] { 2, "FREESHOT", true, true, 1, null, 1 });

            migrationBuilder.InsertData(
                table: "promo_codes",
                columns: new[] { "id", "bonus_balance", "code", "is_enabled", "max_uses", "mood_effect", "uses_left" },
                values: new object[,]
                {
                    { 3, 50, "RICHBOY", true, null, null, 999999 },
                    { 4, 50, "GOODMOOD", true, null, "generous", 999999 }
                });

            migrationBuilder.InsertData(
                table: "promo_codes",
                columns: new[] { "id", "code", "is_enabled", "max_uses", "mood_effect", "night_access", "uses_left" },
                values: new object[] { 5, "NIGHT", true, null, null, true, 999999 });

            migrationBuilder.InsertData(
                table: "promo_codes",
                columns: new[] { "id", "code", "is_enabled", "max_uses", "mood_effect", "uses_left" },
                values: new object[] { 6, "LEGEND", true, 5, null, 5 });

            migrationBuilder.InsertData(
                table: "promo_settings",
                columns: new[] { "id", "promo_enabled" },
                values: new object[] { 1, true });

            migrationBuilder.InsertData(
                table: "drink_ingredients",
                columns: new[] { "drink_id", "ingredient_id" },
                values: new object[,]
                {
                    { 1, 2 },
                    { 1, 6 },
                    { 1, 9 },
                    { 2, 1 },
                    { 2, 7 },
                    { 3, 5 },
                    { 3, 8 },
                    { 3, 9 },
                    { 4, 4 },
                    { 4, 6 },
                    { 5, 3 },
                    { 5, 7 },
                    { 6, 1 },
                    { 6, 9 },
                    { 7, 1 },
                    { 7, 9 },
                    { 7, 10 },
                    { 8, 1 },
                    { 8, 2 },
                    { 8, 3 },
                    { 8, 5 },
                    { 8, 6 },
                    { 9, 1 },
                    { 9, 9 },
                    { 9, 10 },
                    { 10, 2 },
                    { 10, 6 },
                    { 10, 8 },
                    { 11, 5 },
                    { 11, 7 },
                    { 11, 8 },
                    { 12, 1 },
                    { 12, 2 },
                    { 12, 10 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_promos_account_id_code",
                table: "account_promos",
                columns: new[] { "account_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_accounts_token",
                table: "accounts",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_drink_ingredients_ingredient_id",
                table: "drink_ingredients",
                column: "ingredient_id");

            migrationBuilder.CreateIndex(
                name: "IX_drinks_name",
                table: "drinks",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ingredients_name",
                table: "ingredients",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_account_id",
                table: "orders",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_promo_codes_code",
                table: "promo_codes",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_promos");

            migrationBuilder.DropTable(
                name: "balances");

            migrationBuilder.DropTable(
                name: "drink_ingredients");

            migrationBuilder.DropTable(
                name: "mood_tracking");

            migrationBuilder.DropTable(
                name: "order_sequence");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "profiles");

            migrationBuilder.DropTable(
                name: "promo_codes");

            migrationBuilder.DropTable(
                name: "promo_settings");

            migrationBuilder.DropTable(
                name: "rate_limits");

            migrationBuilder.DropTable(
                name: "drinks");

            migrationBuilder.DropTable(
                name: "ingredients");

            migrationBuilder.DropTable(
                name: "accounts");
        }
    }
}
