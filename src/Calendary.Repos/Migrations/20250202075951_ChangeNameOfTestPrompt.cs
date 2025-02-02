using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameOfTestPrompt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "TestPrompts",
                newName: "Synthesises");

            migrationBuilder.RenameIndex(
                name: "IX_TestPrompts_PromptId",
                table: "Synthesises",
                newName: "IX_Synthesises_PromptId");

            migrationBuilder.RenameIndex(
                name: "IX_TestPrompts_TrainingId",
                table: "Synthesises",
                newName: "IX_Synthesises_TrainingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Synthesises",
                newName: "TestPrompts");

            migrationBuilder.RenameIndex(
                name: "IX_Synthesises_PromptId",
                table: "TestPrompts",
                newName: "IX_TestPrompts_PromptId");

            migrationBuilder.RenameIndex(
                name: "IX_Synthesises_TrainingId",
                table: "TestPrompts",
                newName: "IX_TestPrompts_TrainingId");
        }
    }
}
