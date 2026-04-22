using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Instrutores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Especialidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrutores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanosTreino",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "int", nullable: false),
                    Objetivo = table.Column<int>(type: "int", nullable: false),
                    Nivel = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosTreino", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Salas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CapacidadeMaxima = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Ativa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilizadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    DataRegisto = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilizadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Aulas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Categoria = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    Hora = table.Column<TimeOnly>(type: "time", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "int", nullable: false),
                    InstrutorId = table.Column<int>(type: "int", nullable: false),
                    SalaId = table.Column<int>(type: "int", nullable: false),
                    Ativa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aulas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aulas_Instrutores_InstrutorId",
                        column: x => x.InstrutorId,
                        principalTable: "Instrutores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Aulas_Salas_SalaId",
                        column: x => x.SalaId,
                        principalTable: "Salas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Frequencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Entrada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Saida = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UtilizadorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frequencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Frequencias_Utilizadores_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanosTreinoAluno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanoTreinoId = table.Column<int>(type: "int", nullable: false),
                    UtilizadorId = table.Column<int>(type: "int", nullable: false),
                    InstrutorId = table.Column<int>(type: "int", nullable: true),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosTreinoAluno", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanosTreinoAluno_Instrutores_InstrutorId",
                        column: x => x.InstrutorId,
                        principalTable: "Instrutores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PlanosTreinoAluno_PlanosTreino_PlanoTreinoId",
                        column: x => x.PlanoTreinoId,
                        principalTable: "PlanosTreino",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanosTreinoAluno_Utilizadores_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inscricoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataInscricao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UtilizadorId = table.Column<int>(type: "int", nullable: false),
                    AulaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscricoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inscricoes_Aulas_AulaId",
                        column: x => x.AulaId,
                        principalTable: "Aulas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inscricoes_Utilizadores_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Instrutores",
                columns: new[] { "Id", "Ativo", "Email", "Especialidade", "Nome", "Telefone" },
                values: new object[,]
                {
                    { 1, true, "carlos@gym.pt", "Musculação", "Carlos Silva", "961000001" },
                    { 2, true, "ana@gym.pt", "Yoga / Pilates", "Ana Ferreira", "961000002" },
                    { 3, true, "rui@gym.pt", "Spinning / Cardio", "Rui Costa", "961000003" }
                });

            migrationBuilder.InsertData(
                table: "PlanosTreino",
                columns: new[] { "Id", "Ativo", "DataCriacao", "Descricao", "DuracaoMinutos", "Nivel", "Nome", "Objetivo" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Plano para quem está a começar. Foco em técnica e adaptação.", 60, 0, "Iniciação à Musculação", 0 },
                    { 2, true, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Combinação de cardio e musculação para queima calórica.", 75, 1, "Perda de Peso", 1 },
                    { 3, true, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Treino de força e hipertrofia muscular progressiva.", 90, 2, "Ganho de Massa", 2 }
                });

            migrationBuilder.InsertData(
                table: "Salas",
                columns: new[] { "Id", "Ativa", "CapacidadeMaxima", "Descricao", "Nome" },
                values: new object[,]
                {
                    { 1, true, 20, "Sala de musculação e pesos", "Sala A" },
                    { 2, true, 15, "Sala de yoga e pilates", "Sala B" },
                    { 3, true, 25, "Sala de spinning e cardio", "Sala C" }
                });

            migrationBuilder.InsertData(
                table: "Utilizadores",
                columns: new[] { "Id", "Ativo", "DataRegisto", "Email", "Nome", "PasswordHash", "Role", "Telefone" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@gym.pt", "Administrador", "$2a$11$1DV1gt4DimgCp5WP48l.EuPYufyZxhuj2uUD6XJcv5tR0ak5cOhIa", 0, null },
                    { 2, true, new DateTime(2024, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "cliente@gym.pt", "João Cliente", "$2a$11$1DV1gt4DimgCp5WP48l.EuPYufyZxhuj2uUD6XJcv5tR0ak5cOhIa", 1, "912345678" }
                });

            migrationBuilder.InsertData(
                table: "Aulas",
                columns: new[] { "Id", "Ativa", "Categoria", "DiaSemana", "DuracaoMinutos", "Hora", "InstrutorId", "Nome", "SalaId" },
                values: new object[,]
                {
                    { 1, true, 4, 1, 60, new TimeOnly(18, 0, 0), 1, "Musculação Avançada", 1 },
                    { 2, true, 1, 2, 45, new TimeOnly(19, 0, 0), 2, "Yoga Relaxante", 2 },
                    { 3, true, 3, 3, 50, new TimeOnly(20, 0, 0), 3, "Spinning Intensivo", 3 },
                    { 4, true, 2, 4, 45, new TimeOnly(9, 0, 0), 2, "Pilates Manhã", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aulas_InstrutorId",
                table: "Aulas",
                column: "InstrutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Aulas_SalaId",
                table: "Aulas",
                column: "SalaId");

            migrationBuilder.CreateIndex(
                name: "IX_Frequencias_UtilizadorId",
                table: "Frequencias",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscricoes_AulaId",
                table: "Inscricoes",
                column: "AulaId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscricoes_UtilizadorId_AulaId",
                table: "Inscricoes",
                columns: new[] { "UtilizadorId", "AulaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanosTreinoAluno_InstrutorId",
                table: "PlanosTreinoAluno",
                column: "InstrutorId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanosTreinoAluno_PlanoTreinoId",
                table: "PlanosTreinoAluno",
                column: "PlanoTreinoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanosTreinoAluno_UtilizadorId",
                table: "PlanosTreinoAluno",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizadores_Email",
                table: "Utilizadores",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Frequencias");

            migrationBuilder.DropTable(
                name: "Inscricoes");

            migrationBuilder.DropTable(
                name: "PlanosTreinoAluno");

            migrationBuilder.DropTable(
                name: "Aulas");

            migrationBuilder.DropTable(
                name: "PlanosTreino");

            migrationBuilder.DropTable(
                name: "Utilizadores");

            migrationBuilder.DropTable(
                name: "Instrutores");

            migrationBuilder.DropTable(
                name: "Salas");
        }
    }
}
