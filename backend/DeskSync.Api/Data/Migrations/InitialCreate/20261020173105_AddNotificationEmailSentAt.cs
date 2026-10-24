using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace DeskSync.Api.Data.Migrations.InitialCreate
{
    /// <inheritdoc />
    public partial class AddNotificationEmailSentAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "EmailSentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailSentAt",
                table: "Notifications");
        }
    }
}
