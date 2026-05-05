using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StrategyMonitorIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_strategy_monitors_chat_id",
                table: "strategy_monitors");

            migrationBuilder.DropIndex(
                name: "IX_strategy_monitors_trade_strategy_id",
                table: "strategy_monitors");

            migrationBuilder.DropIndex(
                name: "IX_strategy_monitors_stock_id",
                table: "strategy_monitors");

            migrationBuilder.CreateIndex(
                name: "ux_strategy_monitors_active_chat_strategy_stock",
                table: "strategy_monitors",
                columns: new[] { "chat_id", "trade_strategy_id", "stock_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_strategy_monitors_active_chat_strategy_stock",
                table: "strategy_monitors");

            migrationBuilder.CreateIndex(
                name: "IX_strategy_monitors_chat_id",
                table: "strategy_monitors",
                column: "chat_id");

            migrationBuilder.CreateIndex(
                name: "IX_strategy_monitors_stock_id",
                table: "strategy_monitors",
                column: "stock_id");

            migrationBuilder.CreateIndex(
                name: "IX_strategy_monitors_trade_strategy_id",
                table: "strategy_monitors",
                column: "trade_strategy_id");
        }
    }
}
