using Microsoft.EntityFrameworkCore.Migrations;

namespace Covers.Persistency.Migrations
{
    public partial class AddSpotifyPropertiesToEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SpotifyId",
                table: "Tracks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpotifyUri",
                table: "Tracks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpotifyId",
                table: "Albums",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpotifyUri",
                table: "Albums",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpotifyId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "SpotifyUri",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "SpotifyId",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "SpotifyUri",
                table: "Albums");
        }
    }
}
