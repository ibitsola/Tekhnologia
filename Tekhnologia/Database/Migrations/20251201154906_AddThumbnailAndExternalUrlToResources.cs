using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tekhnologia.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddThumbnailAndExternalUrlToResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalUrl",
                table: "DigitalResources",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "DigitalResources",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalUrl",
                table: "DigitalResources");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "DigitalResources");
        }
    }
}
