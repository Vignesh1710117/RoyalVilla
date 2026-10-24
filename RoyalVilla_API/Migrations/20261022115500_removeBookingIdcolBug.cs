using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoyalVilla_API.Migrations
{
    /// <inheritdoc />
    public partial class removeBookingIdcolBug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guests_BookVillas_BookingId",
                table: "Guests");

            migrationBuilder.DropIndex(
                name: "IX_Guests_BookingId",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "Guests");

            migrationBuilder.CreateIndex(
                name: "IX_Guests_BookId",
                table: "Guests",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_Guests_BookVillas_BookId",
                table: "Guests",
                column: "BookId",
                principalTable: "BookVillas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guests_BookVillas_BookId",
                table: "Guests");

            migrationBuilder.DropIndex(
                name: "IX_Guests_BookId",
                table: "Guests");

            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "Guests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Guests_BookingId",
                table: "Guests",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Guests_BookVillas_BookingId",
                table: "Guests",
                column: "BookingId",
                principalTable: "BookVillas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
