using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models.DTOs
{
    // ───── Auth ─────
    public class LoginDto
    {
        [Required] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required, StringLength(100)] public string Nome { get; set; } = string.Empty;
        [Required, EmailAddress]      public string Email { get; set; } = string.Empty;
        [Required, MinLength(6)]      public string Password { get; set; } = string.Empty;
        public string? Telefone { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
    }

    // ───── Instrutor ─────
    // FIX #3: usar DTO dedicado em vez de expor a entidade diretamente
    public class InstrutorCreateDto
    {
        [Required, StringLength(100, MinimumLength = 2)] public string Nome { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [StringLength(100)] public string? Especialidade { get; set; }
        [Phone] public string? Telefone { get; set; }
        public bool Ativo { get; set; } = true;
    }

    // ───── Sala ─────
    // FIX #3: usar DTO dedicado em vez de expor a entidade diretamente
    public class SalaCreateDto
    {
        [Required, StringLength(100)] public string Nome { get; set; } = string.Empty;
        [Required, Range(1, 500)] public int CapacidadeMaxima { get; set; }
        [StringLength(300)] public string? Descricao { get; set; }
        public bool Ativa { get; set; } = true;
    }

    // ───── Aula ─────
    public class AulaListDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int CategoriaId { get; set; }       // FIX #1/#2: expor ID da categoria para edição
        public int DiaSemana { get; set; }
        public string DiaSemanaLabel { get; set; } = string.Empty;
        public string Hora { get; set; } = string.Empty;
        public string HoraFim { get; set; } = string.Empty;
        public int DuracaoMinutos { get; set; }
        public string Instrutor { get; set; } = string.Empty;
        public int InstrutorId { get; set; }       // FIX #1: expor ID do instrutor para edição
        public string Sala { get; set; } = string.Empty;
        public int SalaId { get; set; }            // FIX #1: expor ID da sala para edição
        public int CapacidadeMaxima { get; set; }
        public int Inscritos { get; set; }
        public int VagasDisponiveis { get; set; }
        public bool Lotada { get; set; }
        public bool Ativa { get; set; }
    }

    public class AulaCreateDto
    {
        [Required] public string Nome { get; set; } = string.Empty;
        [Required] public int Categoria { get; set; } = 99;
        [Required] public int DiaSemana { get; set; }
        [Required] public string Hora { get; set; } = "09:00";
        [Range(15, 180)] public int DuracaoMinutos { get; set; } = 60;
        [Required] public int InstrutorId { get; set; }
        [Required] public int SalaId { get; set; }
    }

    // ───── PlanoTreino ─────
    public class PlanoTreinoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int DuracaoMinutos { get; set; }
        public string Nivel { get; set; } = string.Empty;
        public string Objetivo { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public int TotalAtribuicoes { get; set; }
    }

    // FIX #3: usar DTO dedicado em vez de expor a entidade diretamente
    public class PlanoTreinoCreateDto
    {
        [Required, StringLength(100, MinimumLength = 3)] public string Nome { get; set; } = string.Empty;
        [Required, StringLength(500)] public string Descricao { get; set; } = string.Empty;
        [Required, Range(15, 180)] public int DuracaoMinutos { get; set; }
        public int Nivel { get; set; } = 0;
        public int Objetivo { get; set; } = 0;
        public bool Ativo { get; set; } = true;
    }

    public class PlanoTreinoAlunoDto
    {
        public int Id { get; set; }
        public string PlanoTreino { get; set; } = string.Empty;
        public string Utilizador { get; set; } = string.Empty;
        public string? Instrutor { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string? Observacoes { get; set; }
        public bool Ativo { get; set; }
    }

    // FIX #4: DTO para atribuição de plano, com validação de UtilizadorId
    public class PlanoTreinoAlunoCreateDto
    {
        [Required] public int UtilizadorId { get; set; }
        public int? InstrutorId { get; set; }
        public DateTime? DataFim { get; set; }
        [StringLength(500)] public string? Observacoes { get; set; }
        public bool Ativo { get; set; } = true;
    }

    // ───── Frequencia ─────
    public class FrequenciaDto
    {
        public int Id { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime? Saida { get; set; }
        public string? Observacoes { get; set; }
        public bool EmGinasio { get; set; }
        public string? Duracao { get; set; }
    }
}
