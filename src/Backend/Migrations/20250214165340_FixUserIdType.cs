using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixUserIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_VisionBoardItems_UserId",
                table: "VisionBoardItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_UserId",
                table: "JournalEntries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_AspNetUsers_UserId",
                table: "JournalEntries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VisionBoardItems_AspNetUsers_UserId",
                table: "VisionBoardItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_AspNetUsers_UserId",
                table: "JournalEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_VisionBoardItems_AspNetUsers_UserId",
                table: "VisionBoardItems");

            migrationBuilder.DropIndex(
                name: "IX_VisionBoardItems_UserId",
                table: "VisionBoardItems");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_UserId",
                table: "JournalEntries");
        }
    }
}
