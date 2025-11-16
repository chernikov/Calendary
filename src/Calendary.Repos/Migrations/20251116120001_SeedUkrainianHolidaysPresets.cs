using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class SeedUkrainianHolidaysPresets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed HolidayPreset: Українські державні свята
            migrationBuilder.InsertData(
                table: "HolidayPresets",
                columns: new[] { "Id", "Code", "Type" },
                values: new object[] { 1, "UA_STATE", "State" }
            );

            // Seed HolidayPresetTranslations for UA_STATE
            migrationBuilder.InsertData(
                table: "HolidayPresetTranslations",
                columns: new[] { "Id", "HolidayPresetId", "LanguageId", "Name", "Description" },
                values: new object[,]
                {
                    { 1, 1, 1, "Українські державні свята", "Офіційні державні свята України" },
                    { 2, 1, 2, "Ukrainian Public Holidays", "Official public holidays of Ukraine" }
                }
            );

            // Seed Holidays for UA_STATE preset
            // Holiday IDs start from 1000 to avoid conflicts with existing data
            migrationBuilder.InsertData(
                table: "Holidays",
                columns: new[] { "Id", "HolidayPresetId", "Month", "Day", "IsMovable", "CalculationType", "IsWorkingDay", "Type", "CountryId" },
                values: new object[,]
                {
                    { 1000, 1, 1, 1, false, null, false, "State", 1 },    // Новий рік
                    { 1001, 1, 1, 7, false, null, false, "State", 1 },    // Різдво Христове (православне)
                    { 1002, 1, 3, 8, false, null, false, "State", 1 },    // Міжнародний жіночий день
                    { 1003, 1, 5, 1, false, null, false, "State", 1 },    // День праці
                    { 1004, 1, 5, 9, false, null, false, "State", 1 },    // День перемоги над нацизмом
                    { 1005, 1, 6, 28, false, null, false, "State", 1 },   // День Конституції України
                    { 1006, 1, 8, 24, false, null, false, "State", 1 },   // День Незалежності України
                    { 1007, 1, 10, 14, false, null, false, "State", 1 },  // День захисника України
                    { 1008, 1, 12, 25, false, null, false, "State", 1 },  // Різдво Христове (католицьке)
                    { 1009, 1, 4, 19, true, "Easter_Orthodox", false, "Religious", 1 } // Великдень 2026 (православний)
                }
            );

            // Seed HolidayTranslations for Ukrainian holidays
            migrationBuilder.InsertData(
                table: "HolidayTranslations",
                columns: new[] { "Id", "HolidayId", "LanguageId", "Name" },
                values: new object[,]
                {
                    // Новий рік
                    { 2000, 1000, 1, "Новий рік" },
                    { 2001, 1000, 2, "New Year's Day" },

                    // Різдво православне
                    { 2002, 1001, 1, "Різдво Христове" },
                    { 2003, 1001, 2, "Christmas (Orthodox)" },

                    // Міжнародний жіночий день
                    { 2004, 1002, 1, "Міжнародний жіночий день" },
                    { 2005, 1002, 2, "International Women's Day" },

                    // День праці
                    { 2006, 1003, 1, "День праці" },
                    { 2007, 1003, 2, "Labour Day" },

                    // День перемоги
                    { 2008, 1004, 1, "День перемоги над нацизмом у Другій світовій війні" },
                    { 2009, 1004, 2, "Victory Day over Nazism in World War II" },

                    // День Конституції
                    { 2010, 1005, 1, "День Конституції України" },
                    { 2011, 1005, 2, "Constitution Day of Ukraine" },

                    // День Незалежності
                    { 2012, 1006, 1, "День Незалежності України" },
                    { 2013, 1006, 2, "Independence Day of Ukraine" },

                    // День захисника
                    { 2014, 1007, 1, "День захисника України" },
                    { 2015, 1007, 2, "Defender of Ukraine Day" },

                    // Різдво католицьке
                    { 2016, 1008, 1, "Різдво Христове (григоріанське)" },
                    { 2017, 1008, 2, "Christmas (Catholic)" },

                    // Великдень
                    { 2018, 1009, 1, "Великдень" },
                    { 2019, 1009, 2, "Easter Sunday" }
                }
            );

            // Seed HolidayPreset: Православні свята
            migrationBuilder.InsertData(
                table: "HolidayPresets",
                columns: new[] { "Id", "Code", "Type" },
                values: new object[] { 2, "ORTHODOX", "Religious" }
            );

            // Seed HolidayPresetTranslations for ORTHODOX
            migrationBuilder.InsertData(
                table: "HolidayPresetTranslations",
                columns: new[] { "Id", "HolidayPresetId", "LanguageId", "Name", "Description" },
                values: new object[,]
                {
                    { 3, 2, 1, "Православні церковні свята", "Основні православні релігійні свята" },
                    { 4, 2, 2, "Orthodox Religious Holidays", "Main Orthodox religious holidays" }
                }
            );

            // Seed Holidays for ORTHODOX preset
            migrationBuilder.InsertData(
                table: "Holidays",
                columns: new[] { "Id", "HolidayPresetId", "Month", "Day", "IsMovable", "CalculationType", "IsWorkingDay", "Type", "CountryId" },
                values: new object[,]
                {
                    { 2000, 2, 1, 7, false, null, true, "Religious", 1 },     // Різдво Христове
                    { 2001, 2, 1, 19, false, null, true, "Religious", 1 },    // Водохреще
                    { 2002, 2, 4, 19, true, "Easter_Orthodox", true, "Religious", 1 }, // Великдень 2026
                    { 2003, 2, 5, 28, true, "Easter_Orthodox_Ascension", true, "Religious", 1 }, // Вознесіння (40-й день)
                    { 2004, 2, 6, 7, true, "Easter_Orthodox_Pentecost", true, "Religious", 1 }, // Трійця (50-й день)
                    { 2005, 2, 8, 28, false, null, true, "Religious", 1 },    // Успіння Богородиці
                    { 2006, 2, 12, 19, false, null, true, "Religious", 1 }    // День Святого Миколая
                }
            );

            // Seed HolidayTranslations for Orthodox holidays
            migrationBuilder.InsertData(
                table: "HolidayTranslations",
                columns: new[] { "Id", "HolidayId", "LanguageId", "Name" },
                values: new object[,]
                {
                    // Різдво
                    { 3000, 2000, 1, "Різдво Христове" },
                    { 3001, 2000, 2, "Christmas (Orthodox)" },

                    // Водохреще
                    { 3002, 2001, 1, "Водохреще (Богоявлення)" },
                    { 3003, 2001, 2, "Epiphany (Theophany)" },

                    // Великдень
                    { 3004, 2002, 1, "Великдень" },
                    { 3005, 2002, 2, "Easter Sunday" },

                    // Вознесіння
                    { 3006, 2003, 1, "Вознесіння Господнє" },
                    { 3007, 2003, 2, "Ascension of Jesus" },

                    // Трійця
                    { 3008, 2004, 1, "Трійця (П'ятдесятниця)" },
                    { 3009, 2004, 2, "Trinity Sunday (Pentecost)" },

                    // Успіння
                    { 3010, 2005, 1, "Успіння Пресвятої Богородиці" },
                    { 3011, 2005, 2, "Dormition of the Mother of God" },

                    // Святий Миколай
                    { 3012, 2006, 1, "День Святого Миколая" },
                    { 3013, 2006, 2, "St. Nicholas Day" }
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
                    2000, 2001, 2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009,
                    2010, 2011, 2012, 2013, 2014, 2015, 2016, 2017, 2018, 2019,
                    3000, 3001, 3002, 3003, 3004, 3005, 3006, 3007, 3008, 3009,
                    3010, 3011, 3012, 3013
                }
            );

            // Delete Holidays
            migrationBuilder.DeleteData(
                table: "Holidays",
                keyColumn: "Id",
                keyValues: new object[] {
                    1000, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009,
                    2000, 2001, 2002, 2003, 2004, 2005, 2006
                }
            );

            // Delete HolidayPresetTranslations
            migrationBuilder.DeleteData(
                table: "HolidayPresetTranslations",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4 }
            );

            // Delete HolidayPresets
            migrationBuilder.DeleteData(
                table: "HolidayPresets",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2 }
            );
        }
    }
}
