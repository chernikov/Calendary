using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Calendary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HolidayShortNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770002"));

            migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770003"));

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "Holidays",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770001"),
                column: "ShortName",
                value: "Новий рік");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770004"),
                column: "ShortName",
                value: "День праці");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770005"),
                column: "ShortName",
                value: "День перемоги");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770006"),
                column: "ShortName",
                value: "День Конституції");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770007"),
                column: "ShortName",
                value: "День незалежності");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770008"),
                column: "ShortName",
                value: "День захисників");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770009"),
                column: "ShortName",
                value: "Різдво");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770101"),
                column: "ShortName",
                value: "New Year");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770102"),
                column: "ShortName",
                value: "MLK Day");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770103"),
                column: "ShortName",
                value: "Presidents Day");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770104"),
                column: "ShortName",
                value: "Memorial Day");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770105"),
                column: "ShortName",
                value: "Juneteenth");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770106"),
                column: "ShortName",
                value: "July 4th");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770107"),
                column: "ShortName",
                value: "Labor Day");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770108"),
                column: "ShortName",
                value: "Columbus Day");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770109"),
                column: "ShortName",
                value: "Veterans Day");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770110"),
                column: "ShortName",
                value: "Thanksgiving");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770111"),
                column: "ShortName",
                value: "Christmas");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770201"),
                column: "ShortName",
                value: "Nowy Rok");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770202"),
                column: "ShortName",
                value: "Trzech Króli");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770203"),
                column: "ShortName",
                value: "Wielkanoc");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770204"),
                column: "ShortName",
                value: "Lany Poniedziałek");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770205"),
                column: "ShortName",
                value: "Święto Pracy");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770206"),
                column: "ShortName",
                value: "3 Maja");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770207"),
                column: "ShortName",
                value: "Zielone Świątki");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770208"),
                column: "ShortName",
                value: "Boże Ciało");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770209"),
                column: "ShortName",
                value: "Wniebowzięcie NMP");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770210"),
                column: "ShortName",
                value: "Wsz. Świętych");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770211"),
                column: "ShortName",
                value: "Niepodległości");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770212"),
                column: "ShortName",
                value: "Boże Narodz. I");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770213"),
                column: "ShortName",
                value: "Boże Narodz. II");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770301"),
                column: "ShortName",
                value: "Neujahr");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770302"),
                column: "ShortName",
                value: "Karfreitag");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770303"),
                column: "ShortName",
                value: "Ostermontag");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770304"),
                column: "ShortName",
                value: "Tag der Arbeit");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770305"),
                column: "ShortName",
                value: "Himmelfahrt");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770306"),
                column: "ShortName",
                value: "Pfingstmontag");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770307"),
                column: "ShortName",
                value: "Dt. Einheit");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770308"),
                column: "ShortName",
                value: "Weihnachten I");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770309"),
                column: "ShortName",
                value: "Weihnachten II");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770401"),
                column: "ShortName",
                value: "Obnovy státu");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770402"),
                column: "ShortName",
                value: "Velký pátek");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770403"),
                column: "ShortName",
                value: "Velikonoce");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770404"),
                column: "ShortName",
                value: "Svátek práce");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770405"),
                column: "ShortName",
                value: "Den vítězství");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770406"),
                column: "ShortName",
                value: "Cyril a Metoděj");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770407"),
                column: "ShortName",
                value: "Jan Hus");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770408"),
                column: "ShortName",
                value: "Česká státnost");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770409"),
                column: "ShortName",
                value: "Vznik ČSR");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770410"),
                column: "ShortName",
                value: "Boj za svobodu");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770411"),
                column: "ShortName",
                value: "Štědrý den");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770412"),
                column: "ShortName",
                value: "Vánoce I");

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777770413"),
                column: "ShortName",
                value: "Vánoce II");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "Holidays");

            migrationBuilder.InsertData(
                table: "Holidays",
                columns: new[] { "Id", "Country", "Day", "Month", "Name", "Year" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777770002"), 0, 7, 1, "Різдво Христове (юліанський календар)", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770003"), 0, 8, 3, "Міжнародний жіночий день", 2027 }
                });
        }
    }
}
