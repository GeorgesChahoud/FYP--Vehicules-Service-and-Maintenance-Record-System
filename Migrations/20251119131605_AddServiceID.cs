using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceID",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceID",
                table: "Appointments",
                column: "ServiceID");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Services_ServiceID",
                table: "Appointments",
                column: "ServiceID",
                principalTable: "Services",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Services_ServiceID",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ServiceID",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ServiceID",
                table: "Appointments");
        }
    }
}
