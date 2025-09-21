using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodX.Simple.Migrations
{
    /// <inheritdoc />
    public partial class AddProductBriefTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductBriefs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BenchmarkBrandReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PackageSize = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StorageRequirements = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CountryOfOrigin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsKosherCertified = table.Column<bool>(type: "bit", nullable: false),
                    KosherOrganization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    KosherSymbol = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SpecialAttributes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBriefs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductBriefs");
        }
    }
}
