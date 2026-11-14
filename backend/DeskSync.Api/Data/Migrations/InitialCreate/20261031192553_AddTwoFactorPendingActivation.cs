using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskSync.Api.Data.Migrations.InitialCreate
{
    /// <inheritdoc />
    public partial class AddTwoFactorPendingActivation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorPendingActivation",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TwoFactorPendingActivation",
                table: "Users");
        }
    }
}
