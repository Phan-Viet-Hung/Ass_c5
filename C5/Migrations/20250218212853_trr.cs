using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace C5.Migrations
{
    /// <inheritdoc />
    public partial class trr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoucherCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComboId",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FinalAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VoucherCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "CartItems");
        }
    }
}
