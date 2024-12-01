using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedAndReplicateId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OutputSeed",
                table: "TestPrompts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReplicateId",
                table: "TestPrompts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Seed",
                table: "TestPrompts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OutputSeed",
                table: "JobTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReplicateId",
                table: "JobTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Seed",
                table: "JobTasks",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutputSeed",
                table: "TestPrompts");

            migrationBuilder.DropColumn(
                name: "ReplicateId",
                table: "TestPrompts");

            migrationBuilder.DropColumn(
                name: "Seed",
                table: "TestPrompts");

            migrationBuilder.DropColumn(
                name: "OutputSeed",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "ReplicateId",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "Seed",
                table: "JobTasks");
        }
    }
}
