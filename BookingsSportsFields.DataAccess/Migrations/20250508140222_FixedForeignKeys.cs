using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingsSportsFields.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixedForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "SportsFieldsEntityId",
                schema: "identity",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "WarningInformation",
                schema: "identity",
                table: "SportsFields",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                schema: "identity",
                table: "Locations",
                type: "decimal(18,9)",
                precision: 18,
                scale: 9,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                schema: "identity",
                table: "Locations",
                type: "decimal(18,9)",
                precision: 18,
                scale: 9,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "identity",
                table: "Bookings",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                schema: "identity",
                table: "Bookings",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WarningInformation",
                schema: "identity",
                table: "SportsFields",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                schema: "identity",
                table: "Locations",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,9)",
                oldPrecision: 18,
                oldScale: 9);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                schema: "identity",
                table: "Locations",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,9)",
                oldPrecision: 18,
                oldScale: 9);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "identity",
                table: "Bookings",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                schema: "identity",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

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
    }
}
