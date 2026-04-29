using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models.Entities
{
    /// <summary>
    /// Plano de treino genérico disponível no ginásio.
    /// Pode ser atribuído a alunos específicos via PlanoTreinoAluno.
    /// Completamente independente das aulas de grupo.
    /// </summary>
    public class PlanoTreino
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do plano é obrigatório")]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Nome do Plano")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(500)]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [Range(15, 180, ErrorMessage = "A duração deve estar entre 15 e 180 minutos")]
        [Display(Name = "Duração (minutos)")]
        public int DuracaoMinutos { get; set; }

        [Display(Name = "Objetivo")]
        public ObjetivoTreino Objetivo { get; set; } = ObjetivoTreino.Geral;

        [Display(Name = "Nível de Dificuldade")]
        public NivelDificuldade Nivel { get; set; } = NivelDificuldade.Iniciante;

        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        // Atribuições a alunos específicos
        public virtual ICollection<PlanoTreinoAluno> Atribuicoes { get; set; } = new List<PlanoTreinoAluno>();
    }

    /// <summary>
    /// Atribuição de um plano genérico a um aluno específico,
    /// podendo ter personalizações (observações, instrutor responsável).
    /// </summary>
    public class PlanoTreinoAluno
    {
        [Key]
        public int Id { get; set; }

        public int PlanoTreinoId { get; set; }
        public virtual PlanoTreino? PlanoTreino { get; set; }

        public int UtilizadorId { get; set; }
        public virtual Utilizador? Utilizador { get; set; }

        public int? InstrutorId { get; set; }
        public virtual Instrutor? Instrutor { get; set; }

        [Display(Name = "Data de Início")]
        public DateTime DataInicio { get; set; } = DateTime.Now;

        [Display(Name = "Data de Fim")]
        public DateTime? DataFim { get; set; }

        [StringLength(500)]
        [Display(Name = "Observações / Personalizações")]
        public string? Observacoes { get; set; }

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;
    }

    public enum NivelDificuldade
    {
        [Display(Name = "Iniciante")]  Iniciante  = 0,
        [Display(Name = "Intermédio")] Intermedio = 1,
        [Display(Name = "Avançado")]   Avancado   = 2
    }

    public enum ObjetivoTreino
    {
        [Display(Name = "Geral")]              Geral          = 0,
        [Display(Name = "Perda de Peso")]      PerdaPeso      = 1,
        [Display(Name = "Ganho de Massa")]     GanhoMassa     = 2,
        [Display(Name = "Resistência")]        Resistencia    = 3,
        [Display(Name = "Reabilitação")]       Reabilitacao   = 4,
        [Display(Name = "Manutenção")]         Manutencao     = 5
    }
}
