using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Covers.Persistency.Migrations
{
    public partial class AddingCoverImagesByteArraysToCover : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Path",
                table: "Covers");

            migrationBuilder.AddColumn<byte[]>(
                name: "BackCover",
                table: "Covers",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FrontCover",
                table: "Covers",
                type: "BLOB",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackCover",
                table: "Covers");

            migrationBuilder.DropColumn(
                name: "FrontCover",
                table: "Covers");

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "Covers",
                type: "TEXT",
                nullable: true);
        }
    }
}
