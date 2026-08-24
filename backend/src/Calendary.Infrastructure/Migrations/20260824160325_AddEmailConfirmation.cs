using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationCode",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmationCodeExpiresAtUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailConfirmationCodeExpiresAtUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "Users");
        }
    }
}
