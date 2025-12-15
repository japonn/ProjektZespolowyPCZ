using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkshopManager.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStatusValuesInDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Aktualizacja wartości statusów w bazie danych
            // Stare wartości enum -> Nowe wartości enum:
            // PendingEstimate (0) -> Created (0) - bez zmian
            // ClientApproval (1) -> PendingApproval (1) - bez zmian
            // Accepted (2) -> Approved (2) - bez zmian
            // InProgress (3) -> InProgress (3) - bez zmian
            // Completed (4) -> ReadyForPickup (4) lub Completed (5)

            // Stare "Completed" (4) należy zmienić na nowe "Completed" (5)
            // ponieważ wartość 4 jest teraz "ReadyForPickup"
            migrationBuilder.Sql("UPDATE RepairOrders SET Status = 5 WHERE Status = 4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Nowe "Completed" (5) z powrotem na stare "Completed" (4)
            migrationBuilder.Sql("UPDATE RepairOrders SET Status = 4 WHERE Status = 5");
        }
    }
}
