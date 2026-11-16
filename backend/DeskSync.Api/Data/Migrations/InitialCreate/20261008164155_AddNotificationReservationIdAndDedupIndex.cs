using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskSync.Api.Data.Migrations.InitialCreate
{
    /// <inheritdoc />
    public partial class AddNotificationReservationIdAndDedupIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReservationId",
                table: "Notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ReservationId_Type",
                table: "Notifications",
                columns: new[] { "ReservationId", "Type" },
                unique: true,
                filter: "\"ReservationId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_ReservationId_Type",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ReservationId",
                table: "Notifications");
        }
    }
}
