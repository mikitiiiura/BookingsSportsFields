using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingsSportsFields.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixReviewsRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReviewsId",
                schema: "identity",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ReviewsId",
                schema: "identity",
                table: "Bookings",
                column: "ReviewsId",
                unique: true,
                filter: "[ReviewsId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Reviews_ReviewsId",
                schema: "identity",
                table: "Bookings",
                column: "ReviewsId",
                principalSchema: "identity",
                principalTable: "Reviews",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Reviews_ReviewsId",
                schema: "identity",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ReviewsId",
                schema: "identity",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ReviewsId",
                schema: "identity",
                table: "Bookings");
        }
    }
}
