using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookIt.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedSeparateGeolocationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Establishments");

            migrationBuilder.AddColumn<int>(
                name: "GeolocationId",
                table: "Establishments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Geolocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Geolocations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_GeolocationId",
                table: "Establishments",
                column: "GeolocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Establishments_Geolocations_GeolocationId",
                table: "Establishments",
                column: "GeolocationId",
                principalTable: "Geolocations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Establishments_Geolocations_GeolocationId",
                table: "Establishments");

            migrationBuilder.DropTable(
                name: "Geolocations");

            migrationBuilder.DropIndex(
                name: "IX_Establishments_GeolocationId",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "GeolocationId",
                table: "Establishments");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Establishments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
