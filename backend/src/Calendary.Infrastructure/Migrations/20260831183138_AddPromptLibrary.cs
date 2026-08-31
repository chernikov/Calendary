using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Calendary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_StyleCategories_StyleCategoryId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "StyleCategories");

            migrationBuilder.DropIndex(
                name: "IX_Orders_StyleCategoryId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StyleCategoryId",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "ImageStyleId",
                table: "Sheets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PromptId",
                table: "Sheets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImageStyles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageStyles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromptThemes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptThemes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromptThemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prompts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prompts_PromptThemes_PromptThemeId",
                        column: x => x.PromptThemeId,
                        principalTable: "PromptThemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ImageStyles",
                columns: new[] { "Id", "Name", "SortOrder", "Text" },
                values: new object[,]
                {
                    { new Guid("55555555-5555-5555-5555-555555555501"), "Фотореалізм", 1, "photorealistic photography, cinematic lighting, rich detail" },
                    { new Guid("55555555-5555-5555-5555-555555555502"), "Графіка", 2, "detailed pencil and ink illustration, hand-drawn graphic art, fine linework" },
                    { new Guid("55555555-5555-5555-5555-555555555503"), "Чорно-біле", 3, "black and white photography, dramatic monochrome contrast, timeless mood" },
                    { new Guid("55555555-5555-5555-5555-555555555504"), "3D-мультфільм", 4, "3D animated feature film style, expressive stylized character, vibrant colors, soft lighting" },
                    { new Guid("55555555-5555-5555-5555-555555555505"), "Аніме", 5, "anime art style, clean linework, vivid cel shading, expressive eyes" },
                    { new Guid("55555555-5555-5555-5555-555555555506"), "Комікс", 6, "comic book art style, bold outlines, halftone shading, dynamic composition" }
                });

            migrationBuilder.InsertData(
                table: "PromptThemes",
                columns: new[] { "Id", "Description", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333301"), "вікінг, фараон, самурай, лицар, козак…", "Історія", 1 },
                    { new Guid("33333333-3333-3333-3333-333333333302"), "нуар, вестерн, шпигун, мюзикл…", "Кіно", 2 },
                    { new Guid("33333333-3333-3333-3333-333333333303"), "альпініст, пілот, дайвер, полярник…", "Пригоди", 3 },
                    { new Guid("33333333-3333-3333-3333-333333333304"), "шеф, лікар, диригент, пожежник…", "Професії", 4 }
                });

            migrationBuilder.InsertData(
                table: "Prompts",
                columns: new[] { "Id", "Name", "PromptThemeId", "SortOrder", "Text" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444401"), "Вікінг", new Guid("33333333-3333-3333-3333-333333333301"), 1, "a fierce Viking warrior with authentic period clothing, armor, and props, in a rugged northern landscape" },
                    { new Guid("44444444-4444-4444-4444-444444444402"), "Фараон", new Guid("33333333-3333-3333-3333-333333333301"), 2, "an Egyptian pharaoh in ceremonial regalia with golden ornaments, amid ancient temple architecture" },
                    { new Guid("44444444-4444-4444-4444-444444444403"), "Самурай", new Guid("33333333-3333-3333-3333-333333333301"), 3, "a samurai in traditional lacquered armor with a katana, in a feudal Japanese setting" },
                    { new Guid("44444444-4444-4444-4444-444444444404"), "Лицар", new Guid("33333333-3333-3333-3333-333333333301"), 4, "a medieval knight in polished plate armor with heraldic details, near a stone castle" },
                    { new Guid("44444444-4444-4444-4444-444444444405"), "Козак", new Guid("33333333-3333-3333-3333-333333333301"), 5, "a Ukrainian Cossack with traditional attire, shaved head with an oseledets, and a saber, on the open steppe" },
                    { new Guid("44444444-4444-4444-4444-444444444406"), "Нуар-детектив", new Guid("33333333-3333-3333-3333-333333333302"), 1, "a film noir detective in a trench coat and fedora, moody city streets with dramatic shadows" },
                    { new Guid("44444444-4444-4444-4444-444444444407"), "Ковбой вестерну", new Guid("33333333-3333-3333-3333-333333333302"), 2, "a spaghetti western gunslinger with a poncho and revolver, in a dusty frontier town" },
                    { new Guid("44444444-4444-4444-4444-444444444408"), "Шпигун", new Guid("33333333-3333-3333-3333-333333333302"), 3, "an elegant secret agent in a tailored suit with spy gadgets, in a glamorous casino or rooftop scene" },
                    { new Guid("44444444-4444-4444-4444-444444444409"), "Зірка мюзиклу", new Guid("33333333-3333-3333-3333-333333333302"), 4, "a golden-age musical performer in a dazzling stage costume, under theatrical spotlights" },
                    { new Guid("44444444-4444-4444-4444-444444444410"), "Альпініст", new Guid("33333333-3333-3333-3333-333333333303"), 1, "a mountaineer with climbing gear and ropes, high on a dramatic snowy peak" },
                    { new Guid("44444444-4444-4444-4444-444444444411"), "Пілот", new Guid("33333333-3333-3333-3333-333333333303"), 2, "a bush pilot with a leather jacket and aviator goggles, beside a vintage propeller plane" },
                    { new Guid("44444444-4444-4444-4444-444444444412"), "Дайвер", new Guid("33333333-3333-3333-3333-333333333303"), 3, "a scuba diver with full diving gear, exploring a vivid coral reef underwater" },
                    { new Guid("44444444-4444-4444-4444-444444444413"), "Полярник", new Guid("33333333-3333-3333-3333-333333333303"), 4, "a polar explorer in an expedition parka with sled dogs, amid arctic ice fields" },
                    { new Guid("44444444-4444-4444-4444-444444444414"), "Шеф-кухар", new Guid("33333333-3333-3333-3333-333333333304"), 1, "a head chef in a pristine white uniform plating a dish, in a busy professional kitchen" },
                    { new Guid("44444444-4444-4444-4444-444444444415"), "Лікар", new Guid("33333333-3333-3333-3333-333333333304"), 2, "a doctor in a white coat with a stethoscope, in a bright modern hospital" },
                    { new Guid("44444444-4444-4444-4444-444444444416"), "Диригент", new Guid("33333333-3333-3333-3333-333333333304"), 3, "an orchestra conductor in a tailcoat mid-performance, baton raised before a grand orchestra" },
                    { new Guid("44444444-4444-4444-4444-444444444417"), "Пожежник", new Guid("33333333-3333-3333-3333-333333333304"), 4, "a firefighter in full turnout gear with a helmet, heroic pose near a fire engine" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sheets_ImageStyleId",
                table: "Sheets",
                column: "ImageStyleId");

            migrationBuilder.CreateIndex(
                name: "IX_Sheets_PromptId",
                table: "Sheets",
                column: "PromptId");

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_PromptThemeId",
                table: "Prompts",
                column: "PromptThemeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sheets_ImageStyles_ImageStyleId",
                table: "Sheets",
                column: "ImageStyleId",
                principalTable: "ImageStyles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sheets_Prompts_PromptId",
                table: "Sheets",
                column: "PromptId",
                principalTable: "Prompts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sheets_ImageStyles_ImageStyleId",
                table: "Sheets");

            migrationBuilder.DropForeignKey(
                name: "FK_Sheets_Prompts_PromptId",
                table: "Sheets");

            migrationBuilder.DropTable(
                name: "ImageStyles");

            migrationBuilder.DropTable(
                name: "Prompts");

            migrationBuilder.DropTable(
                name: "PromptThemes");

            migrationBuilder.DropIndex(
                name: "IX_Sheets_ImageStyleId",
                table: "Sheets");

            migrationBuilder.DropIndex(
                name: "IX_Sheets_PromptId",
                table: "Sheets");

            migrationBuilder.DropColumn(
                name: "ImageStyleId",
                table: "Sheets");

            migrationBuilder.DropColumn(
                name: "PromptId",
                table: "Sheets");

            migrationBuilder.AddColumn<Guid>(
                name: "StyleCategoryId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StyleCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StyleCategories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "StyleCategories",
                columns: new[] { "Id", "Code", "Description", "Name", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), "history", "вікінг, фараон, самурай, лицар, козак…", "Історія", 1 },
                    { new Guid("11111111-1111-1111-1111-111111111102"), "cinema", "нуар, вестерн, шпигун, мюзикл…", "Кіно", 2 },
                    { new Guid("11111111-1111-1111-1111-111111111103"), "adventure", "альпініст, пілот, дайвер, полярник…", "Пригоди", 3 },
                    { new Guid("11111111-1111-1111-1111-111111111104"), "professions", "шеф, лікар, диригент, пожежник…", "Професії", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StyleCategoryId",
                table: "Orders",
                column: "StyleCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_StyleCategories_StyleCategoryId",
                table: "Orders",
                column: "StyleCategoryId",
                principalTable: "StyleCategories",
                principalColumn: "Id");
        }
    }
}
