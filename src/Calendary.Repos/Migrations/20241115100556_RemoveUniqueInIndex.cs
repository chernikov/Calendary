using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendary.Repos.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueInIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Видаляємо старий унікальний індекс
            migrationBuilder.DropIndex(
                name: "IX_PaymentInfos_OrderId",
                table: "PaymentInfos");

            // Створюємо новий неунікальний індекс
            migrationBuilder.CreateIndex(
                name: "IX_PaymentInfos_OrderId",
                table: "PaymentInfos",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Видаляємо неунікальний індекс
            migrationBuilder.DropIndex(
                name: "IX_PaymentInfos_OrderId",
                table: "PaymentInfos");

            // Відновлюємо унікальний індекс
            migrationBuilder.CreateIndex(
                name: "IX_PaymentInfos_OrderId",
                table: "PaymentInfos",
                column: "OrderId",
                unique: true);
        }
    }
}
