using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class AddCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Prompts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "FluxModels",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsAlive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "IsAlive", "Name" },
                values: new object[,]
                {
                    { 1, true, "Чоловік" },
                    { 2, true, "Жінка" },
                    { 3, true, "Хлопчик (малюк)" },
                    { 4, true, "Дівчинка (малюк)" },
                    { 5, true, "Хлопчик" },
                    { 6, true, "Дівчинка" },
                    { 7, true, "Чоловік середнього віку" },
                    { 8, true, "Жінка середнього віку" },
                    { 9, true, "Чоловік поважного віку" },
                    { 10, true, "Жінка поважного віку" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_CategoryId",
                table: "Prompts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FluxModels_CategoryId",
                table: "FluxModels",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_FluxModels_Categories_CategoryId",
                table: "FluxModels",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Prompts_Categories_CategoryId",
                table: "Prompts",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FluxModels_Categories_CategoryId",
                table: "FluxModels");

            migrationBuilder.DropForeignKey(
                name: "FK_Prompts_Categories_CategoryId",
                table: "Prompts");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Prompts_CategoryId",
                table: "Prompts");

            migrationBuilder.DropIndex(
                name: "IX_FluxModels_CategoryId",
                table: "FluxModels");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "FluxModels");
        }
    }
}
