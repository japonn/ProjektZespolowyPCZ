using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkshopManager.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddVatRateToRepairOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VatRate",
                table: "RepairOrders",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "RepairOrders");
        }
    }
}
