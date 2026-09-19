using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThumbUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderPhotos_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderPhotos_OrderId",
                table: "OrderPhotos",
                column: "OrderId");

            // Copy every existing order's single photo into the new collection before the source
            // column disappears below, in the same migration transaction. ThumbUrl is pragmatically
            // backfilled to the same value as Url here (SQL can't run the ImageSharp thumbnail
            // pipeline) — functionally correct, just not size-optimized; MediaMigrator generates a
            // real thumbnail on next startup for any row that was still an inline data: URL.
            migrationBuilder.Sql(@"
                INSERT INTO OrderPhotos (Id, OrderId, Url, ThumbUrl, CreatedAtUtc)
                SELECT NEWID(), Id, PhotoUrl, PhotoUrl, CreatedAtUtc
                FROM Orders
                WHERE PhotoUrl IS NOT NULL;
            ");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            // Best-effort restore: PhotoUrl can only ever hold one value, so this picks an
            // arbitrary one (the earliest by CreatedAtUtc) per order. Orders with 2+ photos lose
            // the rest on rollback — acceptable for a Down() escape hatch, not a guaranteed
            // lossless inverse.
            migrationBuilder.Sql(@"
                UPDATE o
                SET o.PhotoUrl = op.Url
                FROM Orders o
                CROSS APPLY (
                    SELECT TOP 1 Url FROM OrderPhotos WHERE OrderId = o.Id ORDER BY CreatedAtUtc
                ) op;
            ");

            migrationBuilder.DropTable(
                name: "OrderPhotos");
        }
    }
}
