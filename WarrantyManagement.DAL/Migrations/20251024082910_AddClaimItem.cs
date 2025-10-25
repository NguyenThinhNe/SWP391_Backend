using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarrantyManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartItems_WarrantyClaims_ClaimId",
                table: "PartItems");

            migrationBuilder.DropIndex(
                name: "IX_PartItems_ClaimId",
                table: "PartItems");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ClaimId",
                table: "PartItems");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "PartItems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ClaimDetails",
                columns: table => new
                {
                    ClaimDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartItemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimDetails", x => x.ClaimDetailId);
                    table.ForeignKey(
                        name: "FK_ClaimDetails_PartItems_PartItemId",
                        column: x => x.PartItemId,
                        principalTable: "PartItems",
                        principalColumn: "PartItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClaimDetails_WarrantyClaims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "WarrantyClaims",
                        principalColumn: "ClaimId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimDetails_ClaimId",
                table: "ClaimDetails",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimDetails_PartItemId",
                table: "ClaimDetails",
                column: "PartItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimDetails");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "PartItems");

            migrationBuilder.AddColumn<int>(
                name: "Cost",
                table: "Parts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimId",
                table: "PartItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PartItems_ClaimId",
                table: "PartItems",
                column: "ClaimId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartItems_WarrantyClaims_ClaimId",
                table: "PartItems",
                column: "ClaimId",
                principalTable: "WarrantyClaims",
                principalColumn: "ClaimId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
