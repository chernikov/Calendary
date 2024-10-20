using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCalendarIdInHolidayAddLanguageCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Holidays_Calendars_CalendarId",
                table: "Holidays");

            migrationBuilder.DropIndex(
                name: "IX_Holidays_CalendarId",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "CalendarId",
                table: "Holidays");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Languages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: 1,
                column: "Code",
                value: "uk-UA");

            migrationBuilder.UpdateData(
                table: "Languages",
                keyColumn: "Id",
                keyValue: 2,
                column: "Code",
                value: "en-EN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Languages");

            migrationBuilder.AddColumn<int>(
                name: "CalendarId",
                table: "Holidays",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_CalendarId",
                table: "Holidays",
                column: "CalendarId");

            migrationBuilder.AddForeignKey(
                name: "FK_Holidays_Calendars_CalendarId",
                table: "Holidays",
                column: "CalendarId",
                principalTable: "Calendars",
                principalColumn: "Id");
        }
    }
}
