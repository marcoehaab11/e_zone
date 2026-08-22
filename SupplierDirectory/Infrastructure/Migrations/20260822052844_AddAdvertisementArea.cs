using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplierDirectory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvertisementArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "SupplierImages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "Advertisements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierImages_AreaId",
                table: "SupplierImages",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_AreaId",
                table: "Advertisements",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_Areas_AreaId",
                table: "Advertisements",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierImages_Areas_AreaId",
                table: "SupplierImages",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_Areas_AreaId",
                table: "Advertisements");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierImages_Areas_AreaId",
                table: "SupplierImages");

            migrationBuilder.DropIndex(
                name: "IX_SupplierImages_AreaId",
                table: "SupplierImages");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_AreaId",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "SupplierImages");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "Advertisements");
        }
    }
}
