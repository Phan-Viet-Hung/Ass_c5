using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace C5.Migrations
{
    /// <inheritdoc />
    public partial class comb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComboItems_Products_ProductId",
                table: "ComboItems");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Combos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_ComboItems_Products_ProductId",
                table: "ComboItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComboItems_Products_ProductId",
                table: "ComboItems");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Combos");

            migrationBuilder.AddForeignKey(
                name: "FK_ComboItems_Products_ProductId",
                table: "ComboItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
