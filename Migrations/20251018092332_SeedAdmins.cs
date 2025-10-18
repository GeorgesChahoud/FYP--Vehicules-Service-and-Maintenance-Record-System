using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdmins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "ID", "Email", "FirstName", "LastName", "Password", "PhoneNumber", "RoleID" },
                values: new object[,]
                {
                    { 1, "georgeschahoud@carhub-garage.com", "Georges", "Chahoud", "GCH@Car#9", "+96103021684", 1 },
                    { 2, "christopherhannanehme@carhub-garage.com", "Christopher", "Hanna Nehme", "CHN@Car#3", "+96181651808", 1 },
                    { 3, "eliasazar@carhub-garage.com", "Elias", "Azar", "EAZ@Car#5", "+96171750758", 1 },
                    { 4, "anthonychahine@carhub-garage.com", "Anthony", "Chahine", "ACH@Car#7", "+96181866298", 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "ID",
                keyValue: 4);
        }
    }
}
