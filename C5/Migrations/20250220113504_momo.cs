using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace C5.Migrations
{
    /// <inheritdoc />
    public partial class momo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderInformation",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderInformation",
                table: "Orders");
        }
    }
}
