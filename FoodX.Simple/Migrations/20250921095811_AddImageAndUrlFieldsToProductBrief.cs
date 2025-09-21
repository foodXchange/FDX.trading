using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodX.Simple.Migrations
{
    /// <inheritdoc />
    public partial class AddImageAndUrlFieldsToProductBrief : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BenchmarkWebsiteUrl",
                table: "ProductBriefs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "ProductBriefs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ProductBriefs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BenchmarkWebsiteUrl",
                table: "ProductBriefs");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "ProductBriefs");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ProductBriefs");
        }
    }
}
