using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePitchSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PitchSlots",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "PitchSlots",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PitchSlots_UserId",
                table: "PitchSlots",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PitchSlots_Users_UserId",
                table: "PitchSlots",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PitchSlots_Users_UserId",
                table: "PitchSlots");

            migrationBuilder.DropIndex(
                name: "IX_PitchSlots_UserId",
                table: "PitchSlots");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PitchSlots");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PitchSlots");
        }
    }
}
