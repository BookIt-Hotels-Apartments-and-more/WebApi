using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookIt.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SeparatedApartmentAndUserRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "RatingId",
                table: "Users",
                newName: "UserRatingId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_RatingId",
                table: "Users",
                newName: "IX_Users_UserRatingId");

            migrationBuilder.RenameColumn(
                name: "RatingId",
                table: "Establishments",
                newName: "ApartmentRatingId");

            migrationBuilder.RenameIndex(
                name: "IX_Establishments_RatingId",
                table: "Establishments",
                newName: "IX_Establishments_ApartmentRatingId");

            migrationBuilder.RenameColumn(
                name: "RatingId",
                table: "Apartments",
                newName: "ApartmentRatingId");

            migrationBuilder.RenameIndex(
                name: "IX_Apartments_RatingId",
                table: "Apartments",
                newName: "IX_Apartments_ApartmentRatingId");

            migrationBuilder.AlterColumn<float>(
                name: "StaffRating",
                table: "Reviews",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<float>(
                name: "PurityRating",
                table: "Reviews",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<float>(
                name: "PriceQualityRating",
                table: "Reviews",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<float>(
                name: "LocationRating",
                table: "Reviews",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<float>(
                name: "FacilitiesRating",
                table: "Reviews",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<float>(
                name: "ComfortRating",
                table: "Reviews",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<float>(
                name: "CustomerStayRating",
                table: "Reviews",
                type: "real",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApartmentRatings",
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
                    table.PrimaryKey("PK_ApartmentRatings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerStayRating = table.Column<float>(type: "real", nullable: false),
                    ReviewCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRatings", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Apartments_ApartmentRatings_ApartmentRatingId",
                table: "Apartments",
                column: "ApartmentRatingId",
                principalTable: "ApartmentRatings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Establishments_ApartmentRatings_ApartmentRatingId",
                table: "Establishments",
                column: "ApartmentRatingId",
                principalTable: "ApartmentRatings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserRatings_UserRatingId",
                table: "Users",
                column: "UserRatingId",
                principalTable: "UserRatings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Apartments_ApartmentRatings_ApartmentRatingId",
                table: "Apartments");

            migrationBuilder.DropForeignKey(
                name: "FK_Establishments_ApartmentRatings_ApartmentRatingId",
                table: "Establishments");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserRatings_UserRatingId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "ApartmentRatings");

            migrationBuilder.DropTable(
                name: "UserRatings");

            migrationBuilder.DropColumn(
                name: "CustomerStayRating",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "UserRatingId",
                table: "Users",
                newName: "RatingId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_UserRatingId",
                table: "Users",
                newName: "IX_Users_RatingId");

            migrationBuilder.RenameColumn(
                name: "ApartmentRatingId",
                table: "Establishments",
                newName: "RatingId");

            migrationBuilder.RenameIndex(
                name: "IX_Establishments_ApartmentRatingId",
                table: "Establishments",
                newName: "IX_Establishments_RatingId");

            migrationBuilder.RenameColumn(
                name: "ApartmentRatingId",
                table: "Apartments",
                newName: "RatingId");

            migrationBuilder.RenameIndex(
                name: "IX_Apartments_ApartmentRatingId",
                table: "Apartments",
                newName: "IX_Apartments_RatingId");

            migrationBuilder.AlterColumn<float>(
                name: "StaffRating",
                table: "Reviews",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "PurityRating",
                table: "Reviews",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "PriceQualityRating",
                table: "Reviews",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "LocationRating",
                table: "Reviews",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "FacilitiesRating",
                table: "Reviews",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "ComfortRating",
                table: "Reviews",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComfortRating = table.Column<float>(type: "real", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FacilitiesRating = table.Column<float>(type: "real", nullable: false),
                    GeneralRating = table.Column<float>(type: "real", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LocationRating = table.Column<float>(type: "real", nullable: false),
                    PriceQualityRating = table.Column<float>(type: "real", nullable: false),
                    PurityRating = table.Column<float>(type: "real", nullable: false),
                    ReviewCount = table.Column<int>(type: "int", nullable: false),
                    StaffRating = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.Id);
                });

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
    }
}
