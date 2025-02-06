using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class ThemeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prompts_PromptThemes_ThemeId",
                table: "Prompts");

            migrationBuilder.AlterColumn<int>(
                name: "ThemeId",
                table: "Prompts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Prompts_PromptThemes_ThemeId",
                table: "Prompts",
                column: "ThemeId",
                principalTable: "PromptThemes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prompts_PromptThemes_ThemeId",
                table: "Prompts");

            migrationBuilder.AlterColumn<int>(
                name: "ThemeId",
                table: "Prompts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Prompts_PromptThemes_ThemeId",
                table: "Prompts",
                column: "ThemeId",
                principalTable: "PromptThemes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
