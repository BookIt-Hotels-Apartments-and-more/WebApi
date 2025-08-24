using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookIt.DAL.Migrations
{
    /// <inheritdoc />
    public partial class LinkedFavoritesToEstablishments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Apartments_ApartmentId",
                table: "Favorites");

            migrationBuilder.RenameColumn(
                name: "ApartmentId",
                table: "Favorites",
                newName: "EstablishmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_ApartmentId",
                table: "Favorites",
                newName: "IX_Favorites_EstablishmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Establishments_EstablishmentId",
                table: "Favorites",
                column: "EstablishmentId",
                principalTable: "Establishments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Establishments_EstablishmentId",
                table: "Favorites");

            migrationBuilder.RenameColumn(
                name: "EstablishmentId",
                table: "Favorites",
                newName: "ApartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_EstablishmentId",
                table: "Favorites",
                newName: "IX_Favorites_ApartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Apartments_ApartmentId",
                table: "Favorites",
                column: "ApartmentId",
                principalTable: "Apartments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
