using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SportBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPitchSlotsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeSlotsSlotId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TimeSlots",
                columns: table => new
                {
                    SlotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlotName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlots", x => x.SlotId);
                });

            migrationBuilder.InsertData(
                table: "TimeSlots",
                columns: new[] { "SlotId", "EndTime", "IsActive", "SlotName", "StartTime" },
                values: new object[,]
                {
                    { 1, new TimeSpan(0, 7, 30, 0, 0), true, "Ca 1", new TimeSpan(0, 6, 0, 0, 0) },
                    { 2, new TimeSpan(0, 9, 0, 0, 0), true, "Ca 2", new TimeSpan(0, 7, 30, 0, 0) },
                    { 3, new TimeSpan(0, 17, 30, 0, 0), true, "Ca 3", new TimeSpan(0, 16, 0, 0, 0) },
                    { 4, new TimeSpan(0, 19, 0, 0, 0), true, "Ca 4 (Vàng)", new TimeSpan(0, 17, 30, 0, 0) },
                    { 5, new TimeSpan(0, 20, 30, 0, 0), true, "Ca 5 (Vàng)", new TimeSpan(0, 19, 0, 0, 0) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TimeSlotsSlotId",
                table: "Bookings",
                column: "TimeSlotsSlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_TimeSlots_TimeSlotsSlotId",
                table: "Bookings",
                column: "TimeSlotsSlotId",
                principalTable: "TimeSlots",
                principalColumn: "SlotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_TimeSlots_TimeSlotsSlotId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "TimeSlots");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TimeSlotsSlotId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TimeSlotsSlotId",
                table: "Bookings");
        }
    }
}
