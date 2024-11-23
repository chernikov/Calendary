using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class AddPaidInFluxModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInfos_Orders_OrderId",
                table: "PaymentInfos");

            migrationBuilder.DropIndex(
                name: "IX_PaymentInfos_OrderId",
                table: "PaymentInfos");

            migrationBuilder.DropColumn(
                name: "ProcessedImageUrl",
                table: "Photos");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "PaymentInfos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "FluxModelId",
                table: "PaymentInfos",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ArchiveUrl",
                table: "FluxModels",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "FluxModels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInfos_FluxModelId",
                table: "PaymentInfos",
                column: "FluxModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInfos_OrderId",
                table: "PaymentInfos",
                column: "OrderId",
                unique: false,
                filter: "[OrderId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInfos_FluxModels_FluxModelId",
                table: "PaymentInfos",
                column: "FluxModelId",
                principalTable: "FluxModels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInfos_Orders_OrderId",
                table: "PaymentInfos",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInfos_FluxModels_FluxModelId",
                table: "PaymentInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInfos_Orders_OrderId",
                table: "PaymentInfos");

            migrationBuilder.DropIndex(
                name: "IX_PaymentInfos_FluxModelId",
                table: "PaymentInfos");

            migrationBuilder.DropIndex(
                name: "IX_PaymentInfos_OrderId",
                table: "PaymentInfos");

            migrationBuilder.DropColumn(
                name: "FluxModelId",
                table: "PaymentInfos");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "FluxModels");

            migrationBuilder.AddColumn<string>(
                name: "ProcessedImageUrl",
                table: "Photos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "PaymentInfos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ArchiveUrl",
                table: "FluxModels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInfos_OrderId",
                table: "PaymentInfos",
                column: "OrderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInfos_Orders_OrderId",
                table: "PaymentInfos",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
