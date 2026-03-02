using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsToChatTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "private_name",
                table: "chats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "activation_token",
                table: "chats",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "private_name",
                table: "chats");

            migrationBuilder.DropColumn(
                name: "activation_token",
                table: "chats");
        }
    }
}
