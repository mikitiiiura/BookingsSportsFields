using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingsSportsFields.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CreatedNotNormalDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SportsFieldImages",
                schema: "identity");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "identity",
                table: "SportsFields",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WarningInformation",
                schema: "identity",
                table: "SportsFields",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                schema: "identity",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SportsFieldsEntityId",
                schema: "identity",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SportsFieldsEntityId",
                schema: "identity",
                table: "Bookings",
                column: "SportsFieldsEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_SportsFields_SportsFieldsEntityId",
                schema: "identity",
                table: "Bookings",
                column: "SportsFieldsEntityId",
                principalSchema: "identity",
                principalTable: "SportsFields",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_SportsFields_SportsFieldsEntityId",
                schema: "identity",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_SportsFieldsEntityId",
                schema: "identity",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "identity",
                table: "SportsFields");

            migrationBuilder.DropColumn(
                name: "WarningInformation",
                schema: "identity",
                table: "SportsFields");

            migrationBuilder.DropColumn(
                name: "Comment",
                schema: "identity",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SportsFieldsEntityId",
                schema: "identity",
                table: "Bookings");

            migrationBuilder.CreateTable(
                name: "SportsFieldImages",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SportsFieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SportsFieldImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SportsFieldImages_SportsFields_SportsFieldId",
                        column: x => x.SportsFieldId,
                        principalSchema: "identity",
                        principalTable: "SportsFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SportsFieldImages_SportsFieldId",
                schema: "identity",
                table: "SportsFieldImages",
                column: "SportsFieldId");
        }
    }
}
