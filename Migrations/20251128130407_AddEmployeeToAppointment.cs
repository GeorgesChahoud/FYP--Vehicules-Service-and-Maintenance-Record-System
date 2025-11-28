using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeID",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_EmployeeID",
                table: "Appointments",
                column: "EmployeeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Employees_EmployeeID",
                table: "Appointments",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Employees_EmployeeID",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_EmployeeID",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "EmployeeID",
                table: "Appointments");
        }
    }
}
