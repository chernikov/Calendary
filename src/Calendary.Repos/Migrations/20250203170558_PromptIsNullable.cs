using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class PromptIsNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Synthesises_Prompts_PromptId",
                table: "Synthesises");

            migrationBuilder.AlterColumn<int>(
                name: "PromptId",
                table: "Synthesises",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Synthesises_Prompts_PromptId",
                table: "Synthesises",
                column: "PromptId",
                principalTable: "Prompts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Synthesises_Prompts_PromptId",
                table: "Synthesises");

            migrationBuilder.AlterColumn<int>(
                name: "PromptId",
                table: "Synthesises",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Synthesises_Prompts_PromptId",
                table: "Synthesises",
                column: "PromptId",
                principalTable: "Prompts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
