using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TradeStrategyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trade_strategies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chat_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    timeframe = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ema_fast_period = table.Column<int>(type: "integer", nullable: false),
                    ema_slow_period = table.Column<int>(type: "integer", nullable: false),
                    adx_period = table.Column<int>(type: "integer", nullable: false),
                    adx_trend_strength_threshold = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    adx_non_falling_look_back_bars = table.Column<int>(type: "integer", nullable: false),
                    atr_period = table.Column<int>(type: "integer", nullable: false),
                    atr_initial_stop_multiplier = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    atr_trailing_stop_multiplier = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    atr_trailing_activation_multiplier = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trade_strategies", x => x.id);
                    table.ForeignKey(
                        name: "fk_trade_strategies_chats_chat_id",
                        column: x => x.chat_id,
                        principalTable: "chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "strategy_monitors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chat_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trade_strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_id = table.Column<Guid>(type: "uuid", nullable: false),
                    symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    timeframe = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    position_side = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    entry_price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    entry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    locked_atr = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    highest_close = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    active_stop = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    trailing_activated = table.Column<bool>(type: "boolean", nullable: false),
                    create_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    stop_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_evaluated_candle_date = table.Column<DateOnly>(type: "date", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_strategy_monitors", x => x.id);
                    table.ForeignKey(
                        name: "fk_strategy_monitors_chats_chat_id",
                        column: x => x.chat_id,
                        principalTable: "chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_strategy_monitors_stocks_stock_id",
                        column: x => x.stock_id,
                        principalTable: "stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_strategy_monitors_trade_strategies_trade_strategy_id",
                        column: x => x.trade_strategy_id,
                        principalTable: "trade_strategies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_trade_strategies_chat_id",
                table: "trade_strategies",
                column: "chat_id");

            migrationBuilder.CreateIndex(
                name: "UX_trade_strategies_chat_name",
                table: "trade_strategies",
                columns: new[] { "chat_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "strategy_monitors");

            migrationBuilder.DropTable(
                name: "trade_strategies");
        }
    }
}
