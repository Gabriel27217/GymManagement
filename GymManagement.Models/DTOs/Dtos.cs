using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models.DTOs
{
    // ───── Autenticação (Auth) ─────

    /// <summary>
    /// Objeto de transferência para a operação de início de sessão (Login).
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Endereço de correio eletrónico do utilizador.
        /// </summary>
        [Required] public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Palavra-passe da conta.
        /// </summary>
        [Required] public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Objeto de transferência para o registo de novos clientes no sistema.
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// Nome completo do novo utilizador.
        /// </summary>
        [Required, StringLength(100)] public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Email que será utilizado para o acesso futuro.
        /// </summary>
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Palavra-passe com um mínimo de 6 caracteres para garantir segurança básica.
        /// </summary>
        [Required, MinLength(6)] public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Número de contacto telefónico (opcional).
        /// </summary>
        public string? Telefone { get; set; }
    }

    /// <summary>
    /// Resposta enviada pelo servidor após uma autenticação bem-sucedida.
    /// Contém o Token JWT e dados básicos do perfil.
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// Token de autenticação (JWT) para aceder a recursos protegidos.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Nome do utilizador autenticado.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Email do utilizador autenticado.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Perfil/Função do utilizador (ex: Admin ou Cliente).
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora de expiração do Token.
        /// </summary>
        public DateTime Expira { get; set; }
    }

    // ───── Aula ─────

    /// <summary>
    /// Representação simplificada de uma aula para exibição em listagens.
    /// </summary>
    public class AulaListDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int DiaSemana { get; set; }

        /// <summary>
        /// Texto amigável do dia da semana (ex: "Segunda-feira").
        /// </summary>
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

    /// <summary>
    /// Dados necessários para a criação de uma nova aula no sistema.
    /// </summary>
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

    // ───── Plano de Treino ─────

    /// <summary>
    /// Detalhes de um plano de treino para consulta e visualização.
    /// </summary>
    public class PlanoTreinoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int DuracaoMinutos { get; set; }
        public string Nivel { get; set; } = string.Empty;
        public string Objetivo { get; set; } = string.Empty;
        public bool Ativo { get; set; }

        /// <summary>
        /// Quantidade total de alunos que possuem este plano atribuído.
        /// </summary>
        public int TotalAtribuicoes { get; set; }
    }

    /// <summary>
    /// Detalhes da atribuição de um plano de treino a um aluno específico.
    /// </summary>
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

    // ───── Frequência ─────

    /// <summary>
    /// Registo de entrada e saída simplificado para o histórico do utilizador.
    /// </summary>
    public class FrequenciaDto
    {
        public int Id { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime? Saida { get; set; }
        public string? Observacoes { get; set; }

        /// <summary>
        /// Indica se o utilizador ainda se encontra presente no ginásio.
        /// </summary>
        public bool EmGinasio { get; set; }

        /// <summary>
        /// Texto formatado representando a duração total da permanência.
        /// </summary>
        public string? Duracao { get; set; }
    }
}