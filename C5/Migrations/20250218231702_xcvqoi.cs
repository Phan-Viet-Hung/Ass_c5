using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace C5.Migrations
{
    /// <inheritdoc />
    public partial class xcvqoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ComboId",
                table: "OrderItems",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ComboId",
                table: "OrderItems",
                column: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Combos_ComboId",
                table: "OrderItems",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Combos_ComboId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ComboId",
                table: "OrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "ComboId",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
