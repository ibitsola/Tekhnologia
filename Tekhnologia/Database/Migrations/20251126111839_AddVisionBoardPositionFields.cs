using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tekhnologia.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddVisionBoardPositionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PositionX",
                table: "VisionBoardItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PositionY",
                table: "VisionBoardItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PositionX",
                table: "VisionBoardItems");

            migrationBuilder.DropColumn(
                name: "PositionY",
                table: "VisionBoardItems");
        }
    }
}
