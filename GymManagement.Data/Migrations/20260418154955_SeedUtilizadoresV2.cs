using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedUtilizadoresV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Utilizadores",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$TwdYRFnMhBqh5J8nZqN9OeGzQGnO7p3k9uJvOBmKhWQE4I3GbUfOi");

            migrationBuilder.UpdateData(
                table: "Utilizadores",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$TwdYRFnMhBqh5J8nZqN9OeGzQGnO7p3k9uJvOBmKhWQE4I3GbUfOi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Utilizadores",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$QiCE/ThU0aoCspQ8gp7Ht.jSi6eEk/VCaMTZiWnDq1rY2z.dprllu");

            migrationBuilder.UpdateData(
                table: "Utilizadores",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$jO299jjkJxdXvdVc8V1/ueLFMSeI2amruLTor4oG6meWhtNIx9ow6");
        }
    }
}
