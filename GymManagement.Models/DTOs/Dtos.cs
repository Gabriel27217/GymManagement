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

    // ───── Aula ─────
    public class AulaListDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int DiaSemana { get; set; }
        public string DiaSemanaLabel { get; set; } = string.Empty;
        public string Hora { get; set; } = string.Empty;
        public string HoraFim { get; set; } = string.Empty;
        public int DuracaoMinutos { get; set; }
        public string Instrutor { get; set; } = string.Empty;
        public string Sala { get; set; } = string.Empty;
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
