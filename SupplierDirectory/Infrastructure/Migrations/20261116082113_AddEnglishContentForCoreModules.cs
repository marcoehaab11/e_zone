using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplierDirectory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnglishContentForCoreModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressEn",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortAddressEn",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetailsEn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "ProductCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "ProductCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AboutEn",
                table: "CompanyInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyNameEn",
                table: "CompanyInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MissionEn",
                table: "CompanyInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlatformDescriptionEn",
                table: "CompanyInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlatformServicesEn",
                table: "CompanyInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisionEn",
                table: "CompanyInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Areas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Areas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Advertisements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "Advertisements",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressEn",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "ShortAddressEn",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DetailsEn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "AboutEn",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "CompanyNameEn",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "MissionEn",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "PlatformDescriptionEn",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "PlatformServicesEn",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "VisionEn",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "Advertisements");
        }
    }
}
