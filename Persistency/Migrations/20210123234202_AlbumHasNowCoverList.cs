using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Covers.Persistency.Migrations
{
    public partial class AlbumHasNowCoverList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Covers_AlbumId",
                table: "Covers");

            migrationBuilder.DropColumn(
                name: "BackCover",
                table: "Covers");

            migrationBuilder.RenameColumn(
                name: "FrontCover",
                table: "Covers",
                newName: "CoverImage");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Covers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Covers_AlbumId",
                table: "Covers",
                column: "AlbumId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Covers_AlbumId",
                table: "Covers");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Covers");

            migrationBuilder.RenameColumn(
                name: "CoverImage",
                table: "Covers",
                newName: "FrontCover");

            migrationBuilder.AddColumn<byte[]>(
                name: "BackCover",
                table: "Covers",
                type: "BLOB",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Covers_AlbumId",
                table: "Covers",
                column: "AlbumId",
                unique: true);
        }
    }
}
