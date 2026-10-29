using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalVilla_API.Migrations
{
    /// <inheritdoc />
    public partial class ChangesInPaymentDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GatewayBookingId",
                table: "Payments",
                newName: "GatewayOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GatewayOrderId",
                table: "Payments",
                newName: "GatewayBookingId");
        }
    }
}
