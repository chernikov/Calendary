using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarToEventDateAndHoliday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventDates_UserSettings_UserSettingId",
                table: "EventDates");

            migrationBuilder.DropForeignKey(
                name: "FK_Holidays_Countries_CountryId",
                table: "Holidays");

            migrationBuilder.AlterColumn<int>(
                name: "CountryId",
                table: "Holidays",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CalendarId",
                table: "Holidays",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserSettingId",
                table: "EventDates",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_CalendarId",
                table: "Holidays",
                column: "CalendarId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventDates_UserSettings_UserSettingId",
                table: "EventDates",
                column: "UserSettingId",
                principalTable: "UserSettings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Holidays_Calendars_CalendarId",
                table: "Holidays",
                column: "CalendarId",
                principalTable: "Calendars",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Holidays_Countries_CountryId",
                table: "Holidays",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventDates_UserSettings_UserSettingId",
                table: "EventDates");

            migrationBuilder.DropForeignKey(
                name: "FK_Holidays_Calendars_CalendarId",
                table: "Holidays");

            migrationBuilder.DropForeignKey(
                name: "FK_Holidays_Countries_CountryId",
                table: "Holidays");

            migrationBuilder.DropIndex(
                name: "IX_Holidays_CalendarId",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "CalendarId",
                table: "Holidays");

            migrationBuilder.AlterColumn<int>(
                name: "CountryId",
                table: "Holidays",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserSettingId",
                table: "EventDates",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EventDates_UserSettings_UserSettingId",
                table: "EventDates",
                column: "UserSettingId",
                principalTable: "UserSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Holidays_Countries_CountryId",
                table: "Holidays",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
