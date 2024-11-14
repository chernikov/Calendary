using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "UserSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryRaw",
                table: "UserSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "DeliveryRaw",
                table: "UserSettings");
        }
    }
}
