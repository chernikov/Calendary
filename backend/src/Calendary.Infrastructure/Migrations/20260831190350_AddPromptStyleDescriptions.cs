using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptStyleDescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Prompts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreviewImageUrl",
                table: "Prompts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ImageStyles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreviewImageUrl",
                table: "ImageStyles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ImageStyles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555501"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Наче справжня фотографія з кінематографічним світлом", null });

            migrationBuilder.UpdateData(
                table: "ImageStyles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555502"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Витончений малюнок олівцем і тушшю від руки", null });

            migrationBuilder.UpdateData(
                table: "ImageStyles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555503"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Драматичний монохром із позачасовим настроєм", null });

            migrationBuilder.UpdateData(
                table: "ImageStyles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555504"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Яскравий персонаж у стилі анімаційного фільму", null });

            migrationBuilder.UpdateData(
                table: "ImageStyles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555505"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Виразна японська анімація з чистими лініями", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444401"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Суворий воїн півночі серед скель і фʼордів", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444402"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Володар Єгипту в золоті серед стародавніх храмів", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444403"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Воїн у лакованих обладунках з катаною в Японії", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444404"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Середньовічний лицар у латах біля камʼяного замку", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444405"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Козак з оселедцем і шаблею у відкритому степу", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444406"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Детектив у плащі на темних вулицях міста", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444407"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Стрілець у пончо в запиленому містечку фронтиру", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444408"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Елегантний агент у костюмі в розкішному казино", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444409"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Артист у блискучому костюмі під світлом прожекторів", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444410"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Підкорювач вершин серед снігу та скель", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444411"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Авіатор у шкіряній куртці біля вінтажного літака", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444412"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Дослідник глибин серед яскравих коралових рифів", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444413"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Мандрівник з їздовими собаками серед арктичних льодів", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444414"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Маестро кухні за роботою над вишуканою стравою", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444415"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Лікар у білому халаті в сучасній клініці", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444416"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Маестро у фраку перед великим оркестром", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444417"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Рятувальник у спорядженні біля пожежної машини", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444418"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Герой у плащі над вогнями нічного міста", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444419"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Шукач пригод серед руїн у тропічних джунглях", null });

            migrationBuilder.UpdateData(
                table: "Prompts",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444420"),
                columns: new[] { "Description", "PreviewImageUrl" },
                values: new object[] { "Космонавт у скафандрі під зоряним небом", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "PreviewImageUrl",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ImageStyles");

            migrationBuilder.DropColumn(
                name: "PreviewImageUrl",
                table: "ImageStyles");
        }
    }
}
