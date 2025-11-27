using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tekhnologia.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddVisionBoardSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "VisionBoardItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "VisionBoardItems",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "VisionBoardItems");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "VisionBoardItems");
        }
    }
}
