using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryForCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "Calendars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_CountryId",
                table: "Calendars",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Calendars_Countries_CountryId",
                table: "Calendars",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calendars_Countries_CountryId",
                table: "Calendars");

            migrationBuilder.DropIndex(
                name: "IX_Calendars_CountryId",
                table: "Calendars");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Calendars");
        }
    }
}
