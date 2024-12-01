using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class MapAgeGenderToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Оновлення для таблиці Prompts
            migrationBuilder.Sql(@"
                UPDATE Prompts
                SET CategoryId = AgeGender + 1
                WHERE AgeGender IS NOT NULL;
            ");

                    // Оновлення для таблиці FluxModels
            migrationBuilder.Sql(@"
                UPDATE FluxModels
                SET CategoryId = AgeGender + 1
                WHERE AgeGender IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
