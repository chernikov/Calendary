using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPublished : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "PromptThemes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "PromptThemes");
        }
    }
}
