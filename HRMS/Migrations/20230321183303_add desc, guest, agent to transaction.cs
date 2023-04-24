using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.Migrations
{
    public partial class adddescguestagenttotransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgentId",
                table: "Transaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuestId",
                table: "Transaction",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_AgentId",
                table: "Transaction",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_GuestId",
                table: "Transaction",
                column: "GuestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Agent_AgentId",
                table: "Transaction",
                column: "AgentId",
                principalTable: "Agent",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Guest_GuestId",
                table: "Transaction",
                column: "GuestId",
                principalTable: "Guest",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Agent_AgentId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Guest_GuestId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_AgentId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_GuestId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "AgentId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "GuestId",
                table: "Transaction");
        }
    }
}
