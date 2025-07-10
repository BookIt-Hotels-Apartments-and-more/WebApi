using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookIt.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedNavigationPropertyToGeolocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Establishments_GeolocationId",
                table: "Establishments");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_GeolocationId",
                table: "Establishments",
                column: "GeolocationId",
                unique: true,
                filter: "[GeolocationId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Establishments_GeolocationId",
                table: "Establishments");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_GeolocationId",
                table: "Establishments",
                column: "GeolocationId");
        }
    }
}
