using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixBookingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_TimeSlots_TimeSlotsSlotId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TimeSlotsSlotId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TimeSlotsSlotId",
                table: "Bookings");

            migrationBuilder.AddColumn<int>(
                name: "SlotId",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SlotId",
                table: "Bookings",
                column: "SlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_TimeSlots_SlotId",
                table: "Bookings",
                column: "SlotId",
                principalTable: "TimeSlots",
                principalColumn: "SlotId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_TimeSlots_SlotId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_SlotId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SlotId",
                table: "Bookings");

            migrationBuilder.AddColumn<int>(
                name: "TimeSlotsSlotId",
                table: "Bookings",
                type: "int",
                nullable: true);

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
    }
}
