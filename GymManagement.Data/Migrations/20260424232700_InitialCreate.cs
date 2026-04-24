using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

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
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Especialidade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DuracaoMinutos = table.Column<int>(type: "int", nullable: false),
                    Nivel = table.Column<int>(type: "int", nullable: false),
                    Objetivo = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
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
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CapacidadeMaxima = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DataRegisto = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
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
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Categoria = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    Hora = table.Column<TimeOnly>(type: "time", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "int", nullable: false),
                    InstrutorId = table.Column<int>(type: "int", nullable: false),
                    SalaId = table.Column<int>(type: "int", nullable: false)
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
                    UtilizadorId = table.Column<int>(type: "int", nullable: false),
                    DataEntrada = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
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
                name: "InscricaoAula",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UtilizadorId = table.Column<int>(type: "int", nullable: false),
                    AulaId = table.Column<int>(type: "int", nullable: false),
                    DataInscricao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InscricaoAula", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InscricaoAula_Aulas_AulaId",
                        column: x => x.AulaId,
                        principalTable: "Aulas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InscricaoAula_Utilizadores_UtilizadorId",
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
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
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
                name: "IX_InscricaoAula_AulaId",
                table: "InscricaoAula",
                column: "AulaId");

            migrationBuilder.CreateIndex(
                name: "IX_InscricaoAula_UtilizadorId_AulaId",
                table: "InscricaoAula",
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

            // Seed data
            migrationBuilder.InsertData(
                table: "Utilizadores",
                columns: new[] { "Id", "Nome", "Email", "PasswordHash", "Role", "Telefone", "Ativo", "DataRegisto" },
                values: new object[,]
                {
                    { 1, "Administrador", "admin@gym.pt", "$2a$11$1DV1gt4DimgCp5WP48l.EuPYufyZxhuj2uUD6XJcv5tR0ak5cOhIa", 0, null, true, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "João Cliente", "cliente@gym.pt", "$2a$11$1DV1gt4DimgCp5WP48l.EuPYufyZxhuj2uUD6XJcv5tR0ak5cOhIa", 1, "912345678", true, new DateTime(2024, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Instrutores",
                columns: new[] { "Id", "Nome", "Email", "Especialidade", "Telefone" },
                values: new object[,]
                {
                    { 1, "Carlos Silva", "carlos@gym.pt", "Musculação", "961000001" },
                    { 2, "Ana Ferreira", "ana@gym.pt", "Yoga / Pilates", "961000002" },
                    { 3, "Rui Costa", "rui@gym.pt", "Spinning / Cardio", "961000003" }
                });

            migrationBuilder.InsertData(
                table: "Salas",
                columns: new[] { "Id", "Nome", "CapacidadeMaxima", "Descricao" },
                values: new object[,]
                {
                    { 1, "Sala A", 20, "Sala de musculação e pesos" },
                    { 2, "Sala B", 15, "Sala de yoga e pilates" },
                    { 3, "Sala C", 25, "Sala de spinning e cardio" }
                });

            migrationBuilder.InsertData(
                table: "PlanosTreino",
                columns: new[] { "Id", "Nome", "Descricao", "DuracaoMinutos", "Nivel", "Objetivo", "DataCriacao", "Ativo" },
                values: new object[,]
                {
                    { 1, "Iniciação à Musculação", "Plano para quem está a começar. Foco em técnica e adaptação.", 60, 0, 0, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { 2, "Perda de Peso", "Combinação de cardio e musculação para queima calórica.", 75, 1, 1, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true },
                    { 3, "Ganho de Massa", "Treino de força e hipertrofia muscular progressiva.", 90, 2, 2, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true }
                });

            migrationBuilder.InsertData(
                table: "Aulas",
                columns: new[] { "Id", "Nome", "Categoria", "DiaSemana", "Hora", "DuracaoMinutos", "InstrutorId", "SalaId" },
                values: new object[,]
                {
                    { 1, "Musculação Avançada", 4, 1, new TimeOnly(18, 0, 0), 60, 1, 1 },
                    { 2, "Yoga Relaxante", 1, 2, new TimeOnly(19, 0, 0), 45, 2, 2 },
                    { 3, "Spinning Intensivo", 3, 3, new TimeOnly(20, 0, 0), 50, 3, 3 },
                    { 4, "Pilates Manhã", 2, 4, new TimeOnly(9, 0, 0), 45, 2, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Frequencias");

            migrationBuilder.DropTable(
                name: "InscricaoAula");

            migrationBuilder.DropTable(
                name: "PlanosTreinoAluno");

            migrationBuilder.DropTable(
                name: "Aulas");

            migrationBuilder.DropTable(
                name: "PlanosTreino");

            migrationBuilder.DropTable(
                name: "Instrutores");

            migrationBuilder.DropTable(
                name: "Salas");

            migrationBuilder.DropTable(
                name: "Utilizadores");
        }
    }
}
