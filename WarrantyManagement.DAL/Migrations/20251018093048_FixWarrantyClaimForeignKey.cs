using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarrantyManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixWarrantyClaimForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarrantyClaims_CustomerVehicles_CustomerVehicleVIN",
                table: "WarrantyClaims");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyClaims_CustomerVehicleVIN",
                table: "WarrantyClaims");

            migrationBuilder.DropColumn(
                name: "CustomerVehicleVIN",
                table: "WarrantyClaims");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_VIN",
                table: "WarrantyClaims",
                column: "VIN");

            migrationBuilder.AddForeignKey(
                name: "FK_WarrantyClaims_CustomerVehicles_VIN",
                table: "WarrantyClaims",
                column: "VIN",
                principalTable: "CustomerVehicles",
                principalColumn: "VIN",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarrantyClaims_CustomerVehicles_VIN",
                table: "WarrantyClaims");

            migrationBuilder.DropIndex(
                name: "IX_WarrantyClaims_VIN",
                table: "WarrantyClaims");

            migrationBuilder.AddColumn<string>(
                name: "CustomerVehicleVIN",
                table: "WarrantyClaims",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyClaims_CustomerVehicleVIN",
                table: "WarrantyClaims",
                column: "CustomerVehicleVIN");

            migrationBuilder.AddForeignKey(
                name: "FK_WarrantyClaims_CustomerVehicles_CustomerVehicleVIN",
                table: "WarrantyClaims",
                column: "CustomerVehicleVIN",
                principalTable: "CustomerVehicles",
                principalColumn: "VIN",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
