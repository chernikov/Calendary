using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSheetVariantsAndPinnedPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SheetVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SheetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SheetVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SheetVariants_Sheets_SheetId",
                        column: x => x.SheetId,
                        principalTable: "Sheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "ActiveVariantId",
                table: "Sheets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PinnedPhotoId",
                table: "Sheets",
                type: "uniqueidentifier",
                nullable: true);

            // Backfill: every already-generated sheet gets one variant carrying its current image,
            // and becomes that variant's active one — so existing orders keep working unchanged
            // once VariantCount is dropped below.
            migrationBuilder.Sql(@"
                INSERT INTO SheetVariants (Id, SheetId, ImageUrl, CreatedAtUtc)
                SELECT NEWID(), Id, ImageUrl, COALESCE(ReadyAtUtc, GETUTCDATE())
                FROM Sheets
                WHERE ImageUrl IS NOT NULL;

                UPDATE s
                SET s.ActiveVariantId = sv.Id
                FROM Sheets s
                CROSS APPLY (
                    SELECT TOP 1 Id FROM SheetVariants WHERE SheetId = s.Id ORDER BY CreatedAtUtc
                ) sv
                WHERE s.ImageUrl IS NOT NULL;
            ");

            migrationBuilder.DropColumn(
                name: "VariantCount",
                table: "Sheets");

            migrationBuilder.CreateIndex(
                name: "IX_Sheets_ActiveVariantId",
                table: "Sheets",
                column: "ActiveVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Sheets_PinnedPhotoId",
                table: "Sheets",
                column: "PinnedPhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_SheetVariants_SheetId",
                table: "SheetVariants",
                column: "SheetId");

            // No ON DELETE action — Orders already cascades to both OrderPhotos and Sheets
            // directly, so a DB-level SET NULL here would be a second cascade path to Sheets,
            // which SQL Server rejects. EF's ClientSetNull (AppDbContext.cs) nulls the FK in
            // memory instead whenever both sides are loaded together (see LoadOwnedOrderAsync).
            migrationBuilder.AddForeignKey(
                name: "FK_Sheets_OrderPhotos_PinnedPhotoId",
                table: "Sheets",
                column: "PinnedPhotoId",
                principalTable: "OrderPhotos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sheets_SheetVariants_ActiveVariantId",
                table: "Sheets",
                column: "ActiveVariantId",
                principalTable: "SheetVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sheets_OrderPhotos_PinnedPhotoId",
                table: "Sheets");

            migrationBuilder.DropForeignKey(
                name: "FK_Sheets_SheetVariants_ActiveVariantId",
                table: "Sheets");

            migrationBuilder.AddColumn<int>(
                name: "VariantCount",
                table: "Sheets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.DropTable(
                name: "SheetVariants");

            migrationBuilder.DropIndex(
                name: "IX_Sheets_ActiveVariantId",
                table: "Sheets");

            migrationBuilder.DropIndex(
                name: "IX_Sheets_PinnedPhotoId",
                table: "Sheets");

            migrationBuilder.DropColumn(
                name: "ActiveVariantId",
                table: "Sheets");

            migrationBuilder.DropColumn(
                name: "PinnedPhotoId",
                table: "Sheets");
        }
    }
}
