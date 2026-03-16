using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PriceAlertsAndStocksUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_stocks_symbol",
                table: "stocks",
                column: "symbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_price_alerts_chat_stock_direction_price",
                table: "price_alerts",
                columns: new[] { "chat_id", "stock_id", "direction", "price" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_stocks_symbol",
                table: "stocks");

            migrationBuilder.DropIndex(
                name: "UX_price_alerts_chat_stock_direction_price",
                table: "price_alerts");
        }
    }
}
