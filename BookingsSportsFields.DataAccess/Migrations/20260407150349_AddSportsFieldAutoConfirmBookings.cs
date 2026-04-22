using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingsSportsFields.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSportsFieldAutoConfirmBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoConfirmBookings",
                schema: "identity",
                table: "SportsFields",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoConfirmBookings",
                schema: "identity",
                table: "SportsFields");
        }
    }
}
