using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PriceAlertsAndChatsIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_price_alerts_chats_chat_id",
                table: "price_alerts");

            migrationBuilder.AddForeignKey(
                name: "fk_price_alerts_chats_chat_id",
                table: "price_alerts",
                column: "chat_id",
                principalTable: "chats",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_price_alerts_chats_chat_id",
                table: "price_alerts");

            migrationBuilder.AddForeignKey(
                name: "fk_price_alerts_chats_chat_id",
                table: "price_alerts",
                column: "chat_id",
                principalTable: "chats",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
