using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OMA.Migrations
{
    /// <inheritdoc />
    public partial class LobbyName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LobbyName",
                table: "Lobbies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LobbyName",
                table: "Lobbies");
        }
    }
}
