using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Models.Entities
{
    /// <summary>
    /// Entidade de associação para gerir a relação muitos-para-muitos entre Utilizadores e Aulas.
    /// Representa o ato de um cliente se inscrever numa aula de grupo específica.
    /// </summary>
    public class InscricaoAula
    {
        /// <summary>
        /// Identificador único da inscrição (Chave Primária).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Data e hora em que o utilizador efetuou a reserva da aula.
        /// </summary>
        [Display(Name = "Data de Inscrição")]
        public DateTime DataInscricao { get; set; } = DateTime.Now;

        /// <summary>
        /// Identificador do utilizador (aluno) que se inscreveu.
        /// </summary>
        [Required]
        public int UtilizadorId { get; set; }

        /// <summary>
        /// Objeto de navegação para os detalhes do utilizador.
        /// </summary>
        public virtual Utilizador? Utilizador { get; set; }

        /// <summary>
        /// Identificador da aula de grupo associada.
        /// </summary>
        [Required]
        public int AulaId { get; set; }

        /// <summary>
        /// Objeto de navegação para os detalhes da aula.
        /// </summary>
        public virtual Aula? Aula { get; set; }
    }

    /// <summary>
    /// Registo individual de acesso às instalações do ginásio (treino livre).
    /// Permite monitorizar o fluxo de pessoas e o tempo de permanência sem necessidade de aula marcada.
    /// </summary>
    public class Frequencia
    {
        /// <summary>
        /// Identificador único do registo de frequência (Chave Primária).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Data e hora de entrada do utilizador no ginásio.
        /// </summary>
        [Display(Name = "Entrada")]
        public DateTime Entrada { get; set; } = DateTime.Now;

        /// <summary>
        /// Data e hora de saída das instalações. Pode ser nulo se o utilizador ainda estiver a treinar.
        /// </summary>
        [Display(Name = "Saída")]
        public DateTime? Saida { get; set; }

        /// <summary>
        /// Notas adicionais sobre o treino ou ocorrências durante a permanência.
        /// </summary>
        [StringLength(200)]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        /// <summary>
        /// Identificador do utilizador que realizou a entrada.
        /// </summary>
        [Required]
        public int UtilizadorId { get; set; }

        /// <summary>
        /// Objeto de navegação para o utilizador correspondente.
        /// </summary>
        public virtual Utilizador? Utilizador { get; set; }

        /// <summary>
        /// Propriedade calculada que devolve o tempo total da sessão de treino.
        /// Não é persistida na base de dados (NotMapped).
        /// </summary>
        [NotMapped]
        public TimeSpan? Duracao => Saida.HasValue ? Saida - Entrada : null;

        /// <summary>
        /// Indica se o utilizador se encontra atualmente dentro do ginásio.
        /// </summary>
        [NotMapped]
        public bool EmGinasio => !Saida.HasValue;
    }
}