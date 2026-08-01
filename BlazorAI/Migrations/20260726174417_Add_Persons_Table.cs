using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BlazorAI.Migrations
{
    /// <inheritdoc />
    public partial class Add_Persons_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Salary = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Persons",
                columns: new[] { "Id", "Email", "IsActive", "Name", "Salary" },
                values: new object[,]
                {
                    { 1, "felipe.gavilan@example.com", true, "Felipe Gavilán", 45000m },
                    { 2, "maria.lopez@example.com", true, "María López", 52000m },
                    { 3, "carlos.rodriguez@example.com", false, "Carlos Rodríguez", 61000m },
                    { 4, "ana.martinez@example.com", false, "Ana Martínez", 48000m },
                    { 5, "luis.gomez@example.com", true, "Luis Gómez", 55000m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Persons");
        }
    }
}
