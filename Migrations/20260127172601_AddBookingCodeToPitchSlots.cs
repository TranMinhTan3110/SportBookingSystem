using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingCodeToPitchSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookingCode",
                table: "PitchSlots",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingCode",
                table: "PitchSlots");
        }
    }
}
