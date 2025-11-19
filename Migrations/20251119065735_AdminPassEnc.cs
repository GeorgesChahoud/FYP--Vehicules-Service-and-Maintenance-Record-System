using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Migrations
{
    /// <inheritdoc />
    public partial class AdminPassEnc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$VR6vwW/QVwDI8GGet8J4M.00B4rDjAHYBuBl4SC3xIsn5ZQ72KXU.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$YLoFs2ydeRPaR5Vjg5twNeiHOVusJypXzv5YrFrSASL9cBkggEyfO");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 3,
                column: "Password",
                value: "$2a$11$Y7B.ir.5SY3eJ46hJkU0JufQtZTdFo13TK5C1i6FVHgQAD6wz68Fu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 4,
                column: "Password",
                value: "$2a$11$n1K1/r8vUJ2pKXA9SmT2qegZbSKm5KQyy1WvAlnK9umNeK3W/4DXC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 1,
                column: "Password",
                value: "Georges@admin0");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 2,
                column: "Password",
                value: "Chris@admin0");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 3,
                column: "Password",
                value: "Elias@admin0");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 4,
                column: "Password",
                value: "Anthony@admin0");
        }
    }
}
