using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplierDirectory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCategoryHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentServiceCategoryId",
                table: "ServiceCategories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_ParentServiceCategoryId",
                table: "ServiceCategories",
                column: "ParentServiceCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceCategories_ServiceCategories_ParentServiceCategoryId",
                table: "ServiceCategories",
                column: "ParentServiceCategoryId",
                principalTable: "ServiceCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceCategories_ServiceCategories_ParentServiceCategoryId",
                table: "ServiceCategories");

            migrationBuilder.DropIndex(
                name: "IX_ServiceCategories_ParentServiceCategoryId",
                table: "ServiceCategories");

            migrationBuilder.DropColumn(
                name: "ParentServiceCategoryId",
                table: "ServiceCategories");
        }
    }
}
