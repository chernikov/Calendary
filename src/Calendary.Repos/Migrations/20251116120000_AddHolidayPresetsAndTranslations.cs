using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class AddHolidayPresetsAndTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create HolidayPresets table
            migrationBuilder.CreateTable(
                name: "HolidayPresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayPresets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HolidayPresets_Code",
                table: "HolidayPresets",
                column: "Code",
                unique: true);

            // Create HolidayPresetTranslations table
            migrationBuilder.CreateTable(
                name: "HolidayPresetTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HolidayPresetId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayPresetTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HolidayPresetTranslations_HolidayPresets_HolidayPresetId",
                        column: x => x.HolidayPresetId,
                        principalTable: "HolidayPresets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HolidayPresetTranslations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HolidayPresetTranslations_HolidayPresetId_LanguageId",
                table: "HolidayPresetTranslations",
                columns: new[] { "HolidayPresetId", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HolidayPresetTranslations_LanguageId",
                table: "HolidayPresetTranslations",
                column: "LanguageId");

            // Add new columns to Holidays table
            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "Holidays",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Day",
                table: "Holidays",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMovable",
                table: "Holidays",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CalculationType",
                table: "Holidays",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsWorkingDay",
                table: "Holidays",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Holidays",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HolidayPresetId",
                table: "Holidays",
                type: "int",
                nullable: true);

            // Make Date nullable
            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Holidays",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            // Make Name nullable
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Holidays",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Add foreign key to HolidayPresets
            migrationBuilder.CreateIndex(
                name: "IX_Holidays_HolidayPresetId",
                table: "Holidays",
                column: "HolidayPresetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Holidays_HolidayPresets_HolidayPresetId",
                table: "Holidays",
                column: "HolidayPresetId",
                principalTable: "HolidayPresets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Create HolidayTranslations table
            migrationBuilder.CreateTable(
                name: "HolidayTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HolidayId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HolidayTranslations_Holidays_HolidayId",
                        column: x => x.HolidayId,
                        principalTable: "Holidays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HolidayTranslations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HolidayTranslations_HolidayId_LanguageId",
                table: "HolidayTranslations",
                columns: new[] { "HolidayId", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HolidayTranslations_LanguageId",
                table: "HolidayTranslations",
                column: "LanguageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HolidayTranslations");

            migrationBuilder.DropTable(
                name: "HolidayPresetTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_Holidays_HolidayPresets_HolidayPresetId",
                table: "Holidays");

            migrationBuilder.DropTable(
                name: "HolidayPresets");

            migrationBuilder.DropIndex(
                name: "IX_Holidays_HolidayPresetId",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "Month",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "Day",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "IsMovable",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "CalculationType",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "IsWorkingDay",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "HolidayPresetId",
                table: "Holidays");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Holidays",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Holidays",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
