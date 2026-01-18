using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeSlotsAndPitchSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PitchSlots",
                columns: table => new
                {
                    PitchSlotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PitchId = table.Column<int>(type: "int", nullable: false),
                    SlotId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PitchSlots", x => x.PitchSlotId);
                    table.ForeignKey(
                        name: "FK_PitchSlots_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId");
                    table.ForeignKey(
                        name: "FK_PitchSlots_Pitches_PitchId",
                        column: x => x.PitchId,
                        principalTable: "Pitches",
                        principalColumn: "PitchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PitchSlots_TimeSlots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "TimeSlots",
                        principalColumn: "SlotId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PitchSlots_BookingId",
                table: "PitchSlots",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_PitchSlots_PitchId",
                table: "PitchSlots",
                column: "PitchId");

            migrationBuilder.CreateIndex(
                name: "IX_PitchSlots_SlotId",
                table: "PitchSlots",
                column: "SlotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PitchSlots");
        }
    }
}
