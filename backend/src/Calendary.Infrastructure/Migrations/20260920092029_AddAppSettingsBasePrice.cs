using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppSettingsBasePrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "AppSettings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222201"),
                column: "BasePrice",
                value: 1600m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "AppSettings");
        }
    }
}
