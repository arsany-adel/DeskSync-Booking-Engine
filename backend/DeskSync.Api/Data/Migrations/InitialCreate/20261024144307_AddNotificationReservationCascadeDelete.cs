using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskSync.Api.Data.Migrations.InitialCreate
{
    /// <inheritdoc />
    public partial class AddNotificationReservationCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Reservations_ReservationId",
                table: "Notifications",
                column: "ReservationId",
                principalTable: "Reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Reservations_ReservationId",
                table: "Notifications");
        }
    }
}
