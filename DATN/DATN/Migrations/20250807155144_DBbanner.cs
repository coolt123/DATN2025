using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN.Migrations
{
    /// <inheritdoc />
    public partial class DBbanner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "banners",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_banners_CategoryId",
                table: "banners",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_banners_categories_CategoryId",
                table: "banners",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_banners_categories_CategoryId",
                table: "banners");

            migrationBuilder.DropIndex(
                name: "IX_banners_CategoryId",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "banners");
        }
    }
}
