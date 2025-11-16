using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class SeedCatholicHolidaysPreset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed HolidayPreset: Католицькі свята
            migrationBuilder.InsertData(
                table: "HolidayPresets",
                columns: new[] { "Id", "Code", "Type" },
                values: new object[] { 3, "CATHOLIC", "Religious" }
            );

            // Seed HolidayPresetTranslations for CATHOLIC
            migrationBuilder.InsertData(
                table: "HolidayPresetTranslations",
                columns: new[] { "Id", "HolidayPresetId", "LanguageId", "Name", "Description" },
                values: new object[,]
                {
                    { 5, 3, 1, "Католицькі церковні свята", "Основні католицькі релігійні свята" },
                    { 6, 3, 2, "Catholic Religious Holidays", "Main Catholic religious holidays" }
                }
            );

            // Seed Holidays for CATHOLIC preset
            // Catholic Easter 2026: April 5
            migrationBuilder.InsertData(
                table: "Holidays",
                columns: new[] { "Id", "HolidayPresetId", "Month", "Day", "IsMovable", "CalculationType", "IsWorkingDay", "Type", "CountryId" },
                values: new object[,]
                {
                    { 3000, 3, 1, 1, false, null, true, "Religious", null },    // Новий рік
                    { 3001, 3, 1, 6, false, null, true, "Religious", null },    // Богоявлення
                    { 3002, 3, 2, 18, true, "Easter_Catholic_AshWednesday", true, "Religious", null }, // Попільна середа 2026
                    { 3003, 3, 4, 5, true, "Easter_Catholic", true, "Religious", null }, // Великдень католицький 2026
                    { 3004, 3, 5, 14, true, "Easter_Catholic_Ascension", true, "Religious", null }, // Вознесіння (40-й день)
                    { 3005, 3, 5, 24, true, "Easter_Catholic_Pentecost", true, "Religious", null }, // П'ятдесятниця (50-й день)
                    { 3006, 3, 8, 15, false, null, true, "Religious", null },   // Внебовзяття Діви Марії
                    { 3007, 3, 11, 1, false, null, true, "Religious", null },   // День всіх святих
                    { 3008, 3, 12, 25, false, null, false, "Religious", null }  // Різдво
                }
            );

            // Seed HolidayTranslations for Catholic holidays
            migrationBuilder.InsertData(
                table: "HolidayTranslations",
                columns: new[] { "Id", "HolidayId", "LanguageId", "Name" },
                values: new object[,]
                {
                    // Новий рік
                    { 4000, 3000, 1, "Новий рік" },
                    { 4001, 3000, 2, "New Year's Day" },

                    // Богоявлення
                    { 4002, 3001, 1, "Богоявлення" },
                    { 4003, 3001, 2, "Epiphany" },

                    // Попільна середа
                    { 4004, 3002, 1, "Попільна середа" },
                    { 4005, 3002, 2, "Ash Wednesday" },

                    // Великдень католицький
                    { 4006, 3003, 1, "Великдень (католицький)" },
                    { 4007, 3003, 2, "Easter Sunday (Catholic)" },

                    // Вознесіння
                    { 4008, 3004, 1, "Вознесіння Господнє" },
                    { 4009, 3004, 2, "Ascension of Jesus" },

                    // П'ятдесятниця
                    { 4010, 3005, 1, "П'ятдесятниця" },
                    { 4011, 3005, 2, "Pentecost" },

                    // Внебовзяття
                    { 4012, 3006, 1, "Внебовзяття Пресвятої Діви Марії" },
                    { 4013, 3006, 2, "Assumption of the Blessed Virgin Mary" },

                    // День всіх святих
                    { 4014, 3007, 1, "День всіх святих" },
                    { 4015, 3007, 2, "All Saints' Day" },

                    // Різдво
                    { 4016, 3008, 1, "Різдво Христове" },
                    { 4017, 3008, 2, "Christmas Day" }
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete HolidayTranslations
            migrationBuilder.DeleteData(
                table: "HolidayTranslations",
                keyColumn: "Id",
                keyValues: new object[] {
                    4000, 4001, 4002, 4003, 4004, 4005, 4006, 4007, 4008, 4009,
                    4010, 4011, 4012, 4013, 4014, 4015, 4016, 4017
                }
            );

            // Delete Holidays
            migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValues: new object[] {
                    3000, 3001, 3002, 3003, 3004, 3005, 3006, 3007, 3008
                }
            );

            // Delete HolidayPresetTranslations
            migrationBuilder.DeleteData(
                table: "HolidayPresetTranslations",
                keyColumn: "Id",
                keyValues: new object[] { 5, 6 }
            );

            // Delete HolidayPresets
            migrationBuilder.DeleteData(
                table: "HolidayPresets",
                keyColumn: "Id",
                keyValues: new object[] { 3 }
            );
        }
    }
}
