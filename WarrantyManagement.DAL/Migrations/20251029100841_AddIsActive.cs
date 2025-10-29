using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarrantyManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClaimDescription",
                table: "WarrantyClaims");

            migrationBuilder.AlterColumn<string>(
                name: "IssueDescription",
                table: "WarrantyClaims",
                type: "character varying(400)",
                maxLength: 400,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "WarrantyClaims",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isActive",
                table: "WarrantyClaims");

            migrationBuilder.AlterColumn<string>(
                name: "IssueDescription",
                table: "WarrantyClaims",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(400)",
                oldMaxLength: 400);

            migrationBuilder.AddColumn<string>(
                name: "ClaimDescription",
                table: "WarrantyClaims",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
