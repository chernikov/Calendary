using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAgeGender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgeGender",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "AgeGender",
                table: "FluxModels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgeGender",
                table: "Prompts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AgeGender",
                table: "FluxModels",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
