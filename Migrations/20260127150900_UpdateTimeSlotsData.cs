using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SportBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTimeSlotsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 1,
                column: "SlotName",
                value: "Ca Sáng 1 (1.5h)");

            migrationBuilder.UpdateData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 2,
                column: "SlotName",
                value: "Ca Sáng 2 (1.5h)");

            migrationBuilder.UpdateData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 3,
                columns: new[] { "EndTime", "SlotName", "StartTime" },
                values: new object[] { new TimeSpan(0, 10, 30, 0, 0), "Ca Sáng 3 (1.5h)", new TimeSpan(0, 9, 0, 0, 0) });

            migrationBuilder.UpdateData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 4,
                columns: new[] { "EndTime", "SlotName", "StartTime" },
                values: new object[] { new TimeSpan(0, 11, 30, 0, 0), "Ca Trưa 1 (1h)", new TimeSpan(0, 10, 30, 0, 0) });

            migrationBuilder.UpdateData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 5,
                columns: new[] { "EndTime", "SlotName", "StartTime" },
                values: new object[] { new TimeSpan(0, 12, 30, 0, 0), "Ca Trưa 2 (1h)", new TimeSpan(0, 11, 30, 0, 0) });

            migrationBuilder.InsertData(
                table: "TimeSlots",
                columns: new[] { "SlotId", "EndTime", "IsActive", "SlotName", "StartTime" },
                values: new object[,]
                {
                    { 6, new TimeSpan(0, 15, 0, 0, 0), true, "Ca Chiều 1 (1h)", new TimeSpan(0, 14, 0, 0, 0) },
                    { 7, new TimeSpan(0, 16, 0, 0, 0), true, "Ca Chiều 2 (1h)", new TimeSpan(0, 15, 0, 0, 0) },
                    { 8, new TimeSpan(0, 17, 30, 0, 0), true, "Ca Chiều 3 (1.5h)", new TimeSpan(0, 16, 0, 0, 0) },
                    { 9, new TimeSpan(0, 19, 0, 0, 0), true, "Giờ Vàng 1 (1.5h)", new TimeSpan(0, 17, 30, 0, 0) },
                    { 10, new TimeSpan(0, 20, 30, 0, 0), true, "Giờ Vàng 2 (1.5h)", new TimeSpan(0, 19, 0, 0, 0) },
                    { 11, new TimeSpan(0, 22, 0, 0, 0), true, "Ca Đêm 1 (1.5h)", new TimeSpan(0, 20, 30, 0, 0) },
                    { 12, new TimeSpan(0, 23, 0, 0, 0), true, "Ca Vét (1h)", new TimeSpan(0, 22, 0, 0, 0) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 12);

            migrationBuilder.UpdateData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 1,
                column: "SlotName",
                value: "Ca 1");

            migrationBuilder.UpdateData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 2,
                column: "SlotName",
                value: "Ca 2");

            migrationBuilder.UpdateData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 3,
                columns: new[] { "EndTime", "SlotName", "StartTime" },
                values: new object[] { new TimeSpan(0, 17, 30, 0, 0), "Ca 3", new TimeSpan(0, 16, 0, 0, 0) });

            migrationBuilder.UpdateData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 4,
                columns: new[] { "EndTime", "SlotName", "StartTime" },
                values: new object[] { new TimeSpan(0, 19, 0, 0, 0), "Ca 4 (Vàng)", new TimeSpan(0, 17, 30, 0, 0) });

            migrationBuilder.UpdateData(
                table: "TimeSlots",
                keyColumn: "SlotId",
                keyValue: 5,
                columns: new[] { "EndTime", "SlotName", "StartTime" },
                values: new object[] { new TimeSpan(0, 20, 30, 0, 0), "Ca 5 (Vàng)", new TimeSpan(0, 19, 0, 0, 0) });
        }
    }
}
