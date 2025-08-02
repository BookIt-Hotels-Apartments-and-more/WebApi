using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookIt.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedSeparateRatingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RatingId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "ComfortRating",
                table: "Reviews",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "FacilitiesRating",
                table: "Reviews",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "LocationRating",
                table: "Reviews",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "PriceQualityRating",
                table: "Reviews",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "PurityRating",
                table: "Reviews",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "StaffRating",
                table: "Reviews",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "RatingId",
                table: "Establishments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RatingId",
                table: "Apartments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffRating = table.Column<float>(type: "real", nullable: false),
                    PurityRating = table.Column<float>(type: "real", nullable: false),
                    PriceQualityRating = table.Column<float>(type: "real", nullable: false),
                    ComfortRating = table.Column<float>(type: "real", nullable: false),
                    FacilitiesRating = table.Column<float>(type: "real", nullable: false),
                    LocationRating = table.Column<float>(type: "real", nullable: false),
                    GeneralRating = table.Column<float>(type: "real", nullable: false),
                    ReviewCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_RatingId",
                table: "Users",
                column: "RatingId");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_RatingId",
                table: "Establishments",
                column: "RatingId");

            migrationBuilder.CreateIndex(
                name: "IX_Apartments_RatingId",
                table: "Apartments",
                column: "RatingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Apartments_Ratings_RatingId",
                table: "Apartments",
                column: "RatingId",
                principalTable: "Ratings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Establishments_Ratings_RatingId",
                table: "Establishments",
                column: "RatingId",
                principalTable: "Ratings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Ratings_RatingId",
                table: "Users",
                column: "RatingId",
                principalTable: "Ratings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Apartments_Ratings_RatingId",
                table: "Apartments");

            migrationBuilder.DropForeignKey(
                name: "FK_Establishments_Ratings_RatingId",
                table: "Establishments");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Ratings_RatingId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Users_RatingId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Establishments_RatingId",
                table: "Establishments");

            migrationBuilder.DropIndex(
                name: "IX_Apartments_RatingId",
                table: "Apartments");

            migrationBuilder.DropColumn(
                name: "RatingId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ComfortRating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "FacilitiesRating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "LocationRating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "PriceQualityRating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "PurityRating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "StaffRating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RatingId",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "RatingId",
                table: "Apartments");
        }
    }
}
