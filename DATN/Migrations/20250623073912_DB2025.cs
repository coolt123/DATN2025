using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN.Migrations
{
    /// <inheritdoc />
    public partial class DB2025 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "products",
                newName: "NameProduct");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "categories",
                newName: "NameCategory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NameProduct",
                table: "products",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NameCategory",
                table: "categories",
                newName: "Name");
        }
    }
}
