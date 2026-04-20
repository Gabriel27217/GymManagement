using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AulaHorarioSemanal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Utilizadores",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Utilizadores",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "DataHora",
                table: "Aulas");

            migrationBuilder.AddColumn<int>(
                name: "DiaSemana",
                table: "Aulas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Hora",
                table: "Aulas",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.UpdateData(
                table: "Aulas",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DiaSemana", "Hora" },
                values: new object[] { 1, new TimeOnly(18, 0, 0) });

            migrationBuilder.UpdateData(
                table: "Aulas",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DiaSemana", "Hora" },
                values: new object[] { 2, new TimeOnly(19, 0, 0) });

            migrationBuilder.UpdateData(
                table: "Aulas",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DiaSemana", "Hora" },
                values: new object[] { 3, new TimeOnly(20, 0, 0) });

            migrationBuilder.UpdateData(
                table: "Aulas",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DiaSemana", "Hora" },
                values: new object[] { 4, new TimeOnly(9, 0, 0) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaSemana",
                table: "Aulas");

            migrationBuilder.DropColumn(
                name: "Hora",
                table: "Aulas");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataHora",
                table: "Aulas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Aulas",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataHora",
                value: new DateTime(2025, 6, 1, 18, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Aulas",
                keyColumn: "Id",
                keyValue: 2,
                column: "DataHora",
                value: new DateTime(2025, 6, 2, 19, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Aulas",
                keyColumn: "Id",
                keyValue: 3,
                column: "DataHora",
                value: new DateTime(2025, 6, 3, 20, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Aulas",
                keyColumn: "Id",
                keyValue: 4,
                column: "DataHora",
                value: new DateTime(2025, 6, 5, 9, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.InsertData(
                table: "Utilizadores",
                columns: new[] { "Id", "Ativo", "DataRegisto", "Email", "Nome", "PasswordHash", "Role", "Telefone" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@gym.pt", "Administrador", "$2a$11$TwdYRFnMhBqh5J8nZqN9OeGzQGnO7p3k9uJvOBmKhWQE4I3GbUfOi", 0, null },
                    { 2, true, new DateTime(2024, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "cliente@gym.pt", "João Cliente", "$2a$11$TwdYRFnMhBqh5J8nZqN9OeGzQGnO7p3k9uJvOBmKhWQE4I3GbUfOi", 1, "912345678" }
                });
        }
    }
}
