using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Calendary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHolidaysAndOrderCalendarSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HolidayCountries",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValueSql: "N'Ukraine'");

            migrationBuilder.AddColumn<int>(
                name: "WeekStart",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Country = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Holidays",
                columns: new[] { "Id", "Country", "Day", "Month", "Name", "Year" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777770001"), 0, 1, 1, "Новий рік", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770002"), 0, 7, 1, "Різдво Христове (юліанський календар)", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770003"), 0, 8, 3, "Міжнародний жіночий день", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770004"), 0, 1, 5, "День праці", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770005"), 0, 9, 5, "День перемоги над нацизмом у Другій світовій війні", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770006"), 0, 28, 6, "День Конституції України", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770007"), 0, 24, 8, "День незалежності України", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770008"), 0, 1, 10, "День захисників і захисниць України", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770009"), 0, 25, 12, "Різдво Христове (григоріанський календар)", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770101"), 1, 1, 1, "New Year's Day", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770102"), 1, 18, 1, "Martin Luther King Jr. Day", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770103"), 1, 15, 2, "Washington's Birthday", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770104"), 1, 31, 5, "Memorial Day", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770105"), 1, 19, 6, "Juneteenth", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770106"), 1, 4, 7, "Independence Day", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770107"), 1, 6, 9, "Labor Day", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770108"), 1, 11, 10, "Columbus Day", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770109"), 1, 11, 11, "Veterans Day", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770110"), 1, 25, 11, "Thanksgiving Day", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770111"), 1, 25, 12, "Christmas Day", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770201"), 2, 1, 1, "Nowy Rok", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770202"), 2, 6, 1, "Święto Trzech Króli", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770203"), 2, 28, 3, "Wielkanoc", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770204"), 2, 29, 3, "Poniedziałek Wielkanocny", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770205"), 2, 1, 5, "Święto Pracy", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770206"), 2, 3, 5, "Święto Konstytucji 3 Maja", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770207"), 2, 16, 5, "Zielone Świątki", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770208"), 2, 27, 5, "Boże Ciało", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770209"), 2, 15, 8, "Wniebowzięcie Najświętszej Maryi Panny", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770210"), 2, 1, 11, "Wszystkich Świętych", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770211"), 2, 11, 11, "Święto Niepodległości", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770212"), 2, 25, 12, "Boże Narodzenie (pierwszy dzień)", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770213"), 2, 26, 12, "Boże Narodzenie (drugi dzień)", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770301"), 3, 1, 1, "Neujahr", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770302"), 3, 26, 3, "Karfreitag", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770303"), 3, 29, 3, "Ostermontag", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770304"), 3, 1, 5, "Tag der Arbeit", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770305"), 3, 6, 5, "Christi Himmelfahrt", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770306"), 3, 17, 5, "Pfingstmontag", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770307"), 3, 3, 10, "Tag der Deutschen Einheit", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770308"), 3, 25, 12, "1. Weihnachtsfeiertag", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770309"), 3, 26, 12, "2. Weihnachtsfeiertag", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770401"), 4, 1, 1, "Den obnovy samostatného českého státu", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770402"), 4, 26, 3, "Velký pátek", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770403"), 4, 29, 3, "Velikonoční pondělí", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770404"), 4, 1, 5, "Svátek práce", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770405"), 4, 8, 5, "Den vítězství", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770406"), 4, 5, 7, "Den slovanských věrozvěstů Cyrila a Metoděje", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770407"), 4, 6, 7, "Den upálení mistra Jana Husa", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770408"), 4, 28, 9, "Den české státnosti", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770409"), 4, 28, 10, "Den vzniku samostatného československého státu", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770410"), 4, 17, 11, "Den boje za svobodu a demokracii", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770411"), 4, 24, 12, "Štědrý den", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770412"), 4, 25, 12, "1. svátek vánoční", 2027 },
                    { new Guid("77777777-7777-7777-7777-777777770413"), 4, 26, 12, "2. svátek vánoční", 2027 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DropColumn(
                name: "HolidayCountries",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WeekStart",
                table: "Orders");
        }
    }
}
