using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodX.Simple.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWorkflowCompleted",
                table: "ProductBriefs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime?>(
                name: "WorkflowCompletedDate",
                table: "ProductBriefs",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWorkflowCompleted",
                table: "ProductBriefs");

            migrationBuilder.DropColumn(
                name: "WorkflowCompletedDate",
                table: "ProductBriefs");
        }
    }
}
