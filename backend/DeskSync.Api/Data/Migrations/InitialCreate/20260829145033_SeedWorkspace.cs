using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskSync.Api.Data.Migrations.InitialCreate
{
    /// <inheritdoc />
    public partial class SeedWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Workspaces",
                columns: new[] { "Id", "Address", "Description", "GoogleMapsLocation", "LogoUrl", "Name" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "Main HQ Address", "The primary workspace configured via database seeding.", null, null, "Default Workspace" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Workspaces",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
