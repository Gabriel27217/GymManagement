using GymManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Data
{
    /// <summary>
    /// Contexto principal da base de dados GymManagement.
    /// Trabalho académico - Desenvolvimento Web 2024/2025
    /// Autores: 27217, 24634
    /// </summary>
    public class GymDbContext : DbContext
    {
        public GymDbContext(DbContextOptions<GymDbContext> options) : base(options) { }

        // ── DbSets ──────────────────────────────────────────────
        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<PlanoTreino> PlanosTreino { get; set; }
        public DbSet<Instrutor> Instrutores { get; set; }
        public DbSet<Sala> Salas { get; set; }
        public DbSet<Aula> Aulas { get; set; }
        public DbSet<InscricaoAula> Inscricoes { get; set; }
        public DbSet<Frequencia> Frequencias { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ── Índice único no email do utilizador ──────────────
            builder.Entity<Utilizador>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // ── Aula → PlanoTreino (muitos-para-um) ─────────────
            builder.Entity<Aula>()
                .HasOne(a => a.PlanoTreino)
                .WithMany(p => p.Aulas)
                .HasForeignKey(a => a.PlanoTreinoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Aula → Instrutor (muitos-para-um) ───────────────
            builder.Entity<Aula>()
                .HasOne(a => a.Instrutor)
                .WithMany(i => i.Aulas)
                .HasForeignKey(a => a.InstrutorId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Aula → Sala (muitos-para-um) ────────────────────
            builder.Entity<Aula>()
                .HasOne(a => a.Sala)
                .WithMany(s => s.Aulas)
                .HasForeignKey(a => a.SalaId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── InscricaoAula (muitos-para-muitos) ──────────────
            builder.Entity<InscricaoAula>()
                .HasOne(i => i.Utilizador)
                .WithMany(u => u.Inscricoes)
                .HasForeignKey(i => i.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InscricaoAula>()
                .HasOne(i => i.Aula)
                .WithMany(a => a.Inscricoes)
                .HasForeignKey(i => i.AulaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Evitar inscrição duplicada na mesma aula
            builder.Entity<InscricaoAula>()
                .HasIndex(i => new { i.UtilizadorId, i.AulaId })
                .IsUnique();

            // ── Frequencia → Utilizador (muitos-para-um) ────────
            builder.Entity<Frequencia>()
                .HasOne(f => f.Utilizador)
                .WithMany(u => u.Frequencias)
                .HasForeignKey(f => f.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Seed de dados ────────────────────────────────────
            SeedData(builder);
        }

        private static void SeedData(ModelBuilder builder)
        {
            // Utilizadores (temporariamente comentados para evitar hashes dinâmicos do BCrypt nas migrações)
            /*
            builder.Entity<Utilizador>().HasData(
                new Utilizador
                {
                    Id = 1, Nome = "Administrador", Email = "admin@gym.pt",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = TipoUtilizador.Admin, Ativo = true,
                    DataRegisto = new DateTime(2024, 9, 1)
                },
                new Utilizador
                {
                    Id = 2, Nome = "João Cliente", Email = "cliente@gym.pt",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cliente123!"),
                    Role = TipoUtilizador.Cliente, Ativo = true,
                    Telefone = "912345678",
                    DataRegisto = new DateTime(2024, 9, 15)
                }
            );
            */

            // Instrutores
            builder.Entity<Instrutor>().HasData(
                new Instrutor { Id = 1, Nome = "Carlos Silva", Email = "carlos@gym.pt", Especialidade = "Musculação", Telefone = "961000001" },
                new Instrutor { Id = 2, Nome = "Ana Ferreira", Email = "ana@gym.pt", Especialidade = "Yoga", Telefone = "961000002" },
                new Instrutor { Id = 3, Nome = "Rui Costa", Email = "rui@gym.pt", Especialidade = "Spinning", Telefone = "961000003" }
            );

            // Salas
            builder.Entity<Sala>().HasData(
                new Sala { Id = 1, Nome = "Sala A", CapacidadeMaxima = 20, Descricao = "Sala de musculação e pesos" },
                new Sala { Id = 2, Nome = "Sala B", CapacidadeMaxima = 15, Descricao = "Sala de yoga e pilates" },
                new Sala { Id = 3, Nome = "Sala C", CapacidadeMaxima = 25, Descricao = "Sala de spinning e cardio" }
            );

            // Planos de treino
            builder.Entity<PlanoTreino>().HasData(
                new PlanoTreino { Id = 1, Nome = "Musculação", Descricao = "Treino de força e hipertrofia muscular", DuracaoMinutos = 60, Nivel = NivelDificuldade.Intermedio, DataCriacao = new DateTime(2024, 9, 1) },
                new PlanoTreino { Id = 2, Nome = "Yoga", Descricao = "Relaxamento, flexibilidade e equilíbrio", DuracaoMinutos = 45, Nivel = NivelDificuldade.Iniciante, DataCriacao = new DateTime(2024, 9, 1) },
                new PlanoTreino { Id = 3, Nome = "Spinning", Descricao = "Cardio intensivo em bicicleta estática", DuracaoMinutos = 50, Nivel = NivelDificuldade.Avancado, DataCriacao = new DateTime(2024, 9, 1) }
            );

            // Aulas com datas fixas para evitar alterações pendentes no modelo
            builder.Entity<Aula>().HasData(
                new Aula { Id = 1, Nome = "Musculação Avançada", DiaSemana = DiaSemana.Segunda, Hora = new TimeOnly(18, 0), DuracaoMinutos = 60, PlanoTreinoId = 1, InstrutorId = 1, SalaId = 1 },
                new Aula { Id = 2, Nome = "Yoga Relaxante",      DiaSemana = DiaSemana.Terca,   Hora = new TimeOnly(19, 0), DuracaoMinutos = 45, PlanoTreinoId = 2, InstrutorId = 2, SalaId = 2 },
                new Aula { Id = 3, Nome = "Spinning Intensivo",  DiaSemana = DiaSemana.Quarta,  Hora = new TimeOnly(20, 0), DuracaoMinutos = 50, PlanoTreinoId = 3, InstrutorId = 3, SalaId = 3 },
                new Aula { Id = 4, Nome = "Yoga Manhã",          DiaSemana = DiaSemana.Quinta,  Hora = new TimeOnly(9,  0), DuracaoMinutos = 45, PlanoTreinoId = 2, InstrutorId = 2, SalaId = 2 }
            );

            // Inscrição de exemplo
            /*
            builder.Entity<InscricaoAula>().HasData(
                new InscricaoAula { Id = 1, UtilizadorId = 2, AulaId = 1, DataInscricao = new DateTime(2024, 10, 1) },
                new InscricaoAula { Id = 2, UtilizadorId = 2, AulaId = 2, DataInscricao = new DateTime(2024, 10, 1) }
            );
            */
        }
    }
}