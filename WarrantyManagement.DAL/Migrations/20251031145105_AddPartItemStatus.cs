using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarrantyManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPartItemStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PartItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "PartItems");
        }
    }
}
