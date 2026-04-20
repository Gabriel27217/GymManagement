using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedUtilizadores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Utilizadores",
                columns: new[] { "Id", "Ativo", "DataRegisto", "Email", "Nome", "PasswordHash", "Role", "Telefone" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@gym.pt", "Administrador", "$2a$11$QiCE/ThU0aoCspQ8gp7Ht.jSi6eEk/VCaMTZiWnDq1rY2z.dprllu", 0, null },
                    { 2, true, new DateTime(2024, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "cliente@gym.pt", "João Cliente", "$2a$11$jO299jjkJxdXvdVc8V1/ueLFMSeI2amruLTor4oG6meWhtNIx9ow6", 1, "912345678" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Utilizadores",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Utilizadores",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
