using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkshopManager.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalCostFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalCost",
                table: "RepairOrders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AdditionalCostAccepted",
                table: "RepairOrders",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalCostDescription",
                table: "RepairOrders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalCost",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "AdditionalCostAccepted",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "AdditionalCostDescription",
                table: "RepairOrders");
        }
    }
}
