using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarrantyManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "WarrantyClaims");

            migrationBuilder.CreateTable(
                name: "ClaimImages",
                columns: table => new
                {
                    ImageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimImages", x => x.ImageId);
                    table.ForeignKey(
                        name: "FK_ClaimImages_WarrantyClaims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "WarrantyClaims",
                        principalColumn: "ClaimId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimImages_ClaimId",
                table: "ClaimImages",
                column: "ClaimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimImages");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "WarrantyClaims",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
