using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookIt.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedAdditionalRequestsToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalRequests",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalRequests",
                table: "Bookings");
        }
    }
}
