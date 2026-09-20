using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLastDeliveryInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastDeliveryCity",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastDeliveryPhone",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastDeliveryRecipientName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastDeliveryWarehouseAddress",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastDeliveryWarehouseNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastDeliveryCity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastDeliveryPhone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastDeliveryRecipientName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastDeliveryWarehouseAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastDeliveryWarehouseNumber",
                table: "Users");
        }
    }
}
