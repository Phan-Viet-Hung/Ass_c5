using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace C5.Migrations
{
    /// <inheritdoc />
    public partial class xcv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComboId",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "OrderItems");
        }
    }
}
