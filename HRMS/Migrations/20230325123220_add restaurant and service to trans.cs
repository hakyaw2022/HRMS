using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    public partial class addrestaurantandservicetotrans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RestaurantId",
                table: "Transaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "Transaction",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_RestaurantId",
                table: "Transaction",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ServiceId",
                table: "Transaction",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Restaurant_RestaurantId",
                table: "Transaction",
                column: "RestaurantId",
                principalTable: "Restaurant",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Service_ServiceId",
                table: "Transaction",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Restaurant_RestaurantId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Service_ServiceId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_RestaurantId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_ServiceId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "Transaction");
        }
    }
}
