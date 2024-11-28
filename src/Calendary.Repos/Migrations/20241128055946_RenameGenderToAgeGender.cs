using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class RenameGenderToAgeGender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Gender",
                table: "Prompts",
                newName: "AgeGender");

            migrationBuilder.RenameColumn(
                name: "Gender",
                table: "FluxModels",
                newName: "AgeGender");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AgeGender",
                table: "Prompts",
                newName: "Gender");

            migrationBuilder.RenameColumn(
                name: "AgeGender",
                table: "FluxModels",
                newName: "Gender");
        }
    }
}
