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

        public DbSet<Utilizador>        Utilizadores    { get; set; }
        public DbSet<PlanoTreino>       PlanosTreino    { get; set; }
        public DbSet<PlanoTreinoAluno>  PlanosTreinoAluno { get; set; }
        public DbSet<Instrutor>         Instrutores     { get; set; }
        public DbSet<Sala>              Salas           { get; set; }
        public DbSet<Aula>              Aulas           { get; set; }
        public DbSet<InscricaoAula>     Inscricoes      { get; set; }
        public DbSet<Frequencia>        Frequencias     { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Email único
            builder.Entity<Utilizador>()
                .HasIndex(u => u.Email).IsUnique();

            // Aula → Instrutor (muitos-para-um)
            builder.Entity<Aula>()
                .HasOne(a => a.Instrutor)
                .WithMany(i => i.Aulas)
                .HasForeignKey(a => a.InstrutorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Aula → Sala (muitos-para-um)
            builder.Entity<Aula>()
                .HasOne(a => a.Sala)
                .WithMany(s => s.Aulas)
                .HasForeignKey(a => a.SalaId)
                .OnDelete(DeleteBehavior.Restrict);

            // InscricaoAula (muitos-para-muitos Utilizador ↔ Aula)
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

            builder.Entity<InscricaoAula>()
                .HasIndex(i => new { i.UtilizadorId, i.AulaId }).IsUnique();

            // Frequencia → Utilizador
            builder.Entity<Frequencia>()
                .HasOne(f => f.Utilizador)
                .WithMany(u => u.Frequencias)
                .HasForeignKey(f => f.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade);

            // PlanoTreinoAluno → PlanoTreino
            builder.Entity<PlanoTreinoAluno>()
                .HasOne(p => p.PlanoTreino)
                .WithMany(p => p.Atribuicoes)
                .HasForeignKey(p => p.PlanoTreinoId)
                .OnDelete(DeleteBehavior.Cascade);

            // PlanoTreinoAluno → Utilizador
            builder.Entity<PlanoTreinoAluno>()
                .HasOne(p => p.Utilizador)
                .WithMany()
                .HasForeignKey(p => p.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade);

            // PlanoTreinoAluno → Instrutor (opcional)
            builder.Entity<PlanoTreinoAluno>()
                .HasOne(p => p.Instrutor)
                .WithMany()
                .HasForeignKey(p => p.InstrutorId)
                .OnDelete(DeleteBehavior.SetNull);

            SeedData(builder);
        }

        private static void SeedData(ModelBuilder builder)
        {
            // Utilizadores
            builder.Entity<Utilizador>().HasData(
                new Utilizador
                {
                    Id = 1, Nome = "Administrador", Email = "admin@gym.pt",
                    PasswordHash = "$2a$11$1DV1gt4DimgCp5WP48l.EuPYufyZxhuj2uUD6XJcv5tR0ak5cOhIa",
                    Role = TipoUtilizador.Admin, Ativo = true,
                    DataRegisto = new DateTime(2024, 9, 1)
                },
                new Utilizador
                {
                    Id = 2, Nome = "João Cliente", Email = "cliente@gym.pt",
                    PasswordHash = "$2a$11$1DV1gt4DimgCp5WP48l.EuPYufyZxhuj2uUD6XJcv5tR0ak5cOhIa",
                    Role = TipoUtilizador.Cliente, Ativo = true,
                    Telefone = "912345678",
                    DataRegisto = new DateTime(2024, 9, 15)
                }
            );

            // Instrutores
            builder.Entity<Instrutor>().HasData(
                new Instrutor { Id = 1, Nome = "Carlos Silva", Email = "carlos@gym.pt", Especialidade = "Musculação", Telefone = "961000001" },
                new Instrutor { Id = 2, Nome = "Ana Ferreira", Email = "ana@gym.pt",    Especialidade = "Yoga / Pilates", Telefone = "961000002" },
                new Instrutor { Id = 3, Nome = "Rui Costa",   Email = "rui@gym.pt",     Especialidade = "Spinning / Cardio", Telefone = "961000003" }
            );

            // Salas
            builder.Entity<Sala>().HasData(
                new Sala { Id = 1, Nome = "Sala A", CapacidadeMaxima = 20, Descricao = "Sala de musculação e pesos" },
                new Sala { Id = 2, Nome = "Sala B", CapacidadeMaxima = 15, Descricao = "Sala de yoga e pilates" },
                new Sala { Id = 3, Nome = "Sala C", CapacidadeMaxima = 25, Descricao = "Sala de spinning e cardio" }
            );

            // Aulas (sem PlanoTreinoId — independentes)
            builder.Entity<Aula>().HasData(
                new Aula { Id = 1, Nome = "Musculação Avançada", Categoria = CategoriaAula.Musculacao, DiaSemana = DiaSemana.Segunda, Hora = new TimeOnly(18, 0), DuracaoMinutos = 60, InstrutorId = 1, SalaId = 1 },
                new Aula { Id = 2, Nome = "Yoga Relaxante",      Categoria = CategoriaAula.Yoga,       DiaSemana = DiaSemana.Terca,   Hora = new TimeOnly(19, 0), DuracaoMinutos = 45, InstrutorId = 2, SalaId = 2 },
                new Aula { Id = 3, Nome = "Spinning Intensivo",  Categoria = CategoriaAula.Spinning,   DiaSemana = DiaSemana.Quarta,  Hora = new TimeOnly(20, 0), DuracaoMinutos = 50, InstrutorId = 3, SalaId = 3 },
                new Aula { Id = 4, Nome = "Pilates Manhã",       Categoria = CategoriaAula.Pilates,    DiaSemana = DiaSemana.Quinta,  Hora = new TimeOnly(9,  0), DuracaoMinutos = 45, InstrutorId = 2, SalaId = 2 }
            );

            // Planos de treino genéricos
            builder.Entity<PlanoTreino>().HasData(
                new PlanoTreino { Id = 1, Nome = "Iniciação à Musculação", Descricao = "Plano para quem está a começar. Foco em técnica e adaptação.", DuracaoMinutos = 60, Nivel = NivelDificuldade.Iniciante, Objetivo = ObjetivoTreino.Geral, DataCriacao = new DateTime(2024, 9, 1) },
                new PlanoTreino { Id = 2, Nome = "Perda de Peso",          Descricao = "Combinação de cardio e musculação para queima calórica.", DuracaoMinutos = 75, Nivel = NivelDificuldade.Intermedio, Objetivo = ObjetivoTreino.PerdaPeso, DataCriacao = new DateTime(2024, 9, 1) },
                new PlanoTreino { Id = 3, Nome = "Ganho de Massa",         Descricao = "Treino de força e hipertrofia muscular progressiva.", DuracaoMinutos = 90, Nivel = NivelDificuldade.Avancado, Objetivo = ObjetivoTreino.GanhoMassa, DataCriacao = new DateTime(2024, 9, 1) }
            );
        }
    }
}
