using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models.Entities
{
    /// <summary>
    /// Representa um modelo de plano de treino genérico disponibilizado pelo ginásio.
    /// Este plano serve de base para ser atribuído e personalizado para alunos específicos.
    /// </summary>
    public class PlanoTreino
    {
        /// <summary>
        /// Identificador único do plano de treino (Chave Primária).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome descritivo do plano (ex: "Hipertrofia ABC", "Circuito Cardio").
        /// </summary>
        [Required(ErrorMessage = "O nome do plano é obrigatório")]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Nome do Plano")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Detalhes sobre os exercícios ou a metodologia do plano.
        /// </summary>
        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(500)]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Tempo estimado em minutos para a realização do treino.
        /// </summary>
        [Required]
        [Range(15, 180, ErrorMessage = "A duração deve estar entre 15 e 180 minutos")]
        [Display(Name = "Duração (minutos)")]
        public int DuracaoMinutos { get; set; }

        /// <summary>
        /// O foco principal do treino (ex: Perda de Peso, Ganho de Massa).
        /// </summary>
        [Display(Name = "Objetivo")]
        public ObjetivoTreino Objetivo { get; set; } = ObjetivoTreino.Geral;

        /// <summary>
        /// Grau de exigência física do plano de treino.
        /// </summary>
        [Display(Name = "Nível de Dificuldade")]
        public NivelDificuldade Nivel { get; set; } = NivelDificuldade.Iniciante;

        /// <summary>
        /// Data em que o modelo de plano foi inserido no sistema.
        /// </summary>
        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        /// <summary>
        /// Define se o plano ainda pode ser atribuído a novos alunos.
        /// </summary>
        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        /// <summary>
        /// Lista de atribuições deste plano a alunos específicos.
        /// </summary>
        public virtual ICollection<PlanoTreinoAluno> Atribuicoes { get; set; } = new List<PlanoTreinoAluno>();
    }

    /// <summary>
    /// Entidade de associação que liga um Plano de Treino genérico a um Aluno (Utilizador).
    /// Permite a personalização e o acompanhamento temporal do plano para cada indivíduo.
    /// </summary>
    public class PlanoTreinoAluno
    {
        /// <summary>
        /// Identificador único da atribuição (Chave Primária).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID do plano de treino base.
        /// </summary>
        public int PlanoTreinoId { get; set; }
        /// <summary>
        /// Objeto de navegação para o plano de treino associado.
        /// </summary>
        public virtual PlanoTreino? PlanoTreino { get; set; }

        /// <summary>
        /// ID do aluno (Utilizador) que irá realizar o plano.
        /// </summary>
        public int UtilizadorId { get; set; }
        /// <summary>
        /// Objeto de navegação para o aluno associado.
        /// </summary>
        public virtual Utilizador? Utilizador { get; set; }

        /// <summary>
        /// ID do instrutor responsável por acompanhar esta atribuição específica.
        /// </summary>
        public int? InstrutorId { get; set; }
        /// <summary>
        /// Objeto de navegação para o instrutor responsável.
        /// </summary>
        public virtual Instrutor? Instrutor { get; set; }

        /// <summary>
        /// Data em que o aluno deve começar a praticar este plano.
        /// </summary>
        [Display(Name = "Data de Início")]
        public DateTime DataInicio { get; set; } = DateTime.Now;

        /// <summary>
        /// Data prevista ou real para a conclusão ou revisão do plano.
        /// </summary>
        [Display(Name = "Data de Fim")]
        public DateTime? DataFim { get; set; }

        /// <summary>
        /// Notas adicionais adaptadas às necessidades específicas do aluno.
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Observações / Personalizações")]
        public string? Observacoes { get; set; }

        /// <summary>
        /// Indica se este plano específico ainda está em vigor para o aluno.
        /// </summary>
        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;
    }

    /// <summary>
    /// Enumeração dos níveis de dificuldade técnica e física.
    /// </summary>
    public enum NivelDificuldade
    {
        [Display(Name = "Iniciante")] Iniciante = 0,
        [Display(Name = "Intermédio")] Intermedio = 1,
        [Display(Name = "Avançado")] Avancado = 2
    }

    /// <summary>
    /// Enumeração das finalidades principais de um plano de treino.
    /// </summary>
    public enum ObjetivoTreino
    {
        [Display(Name = "Geral")] Geral = 0,
        [Display(Name = "Perda de Peso")] PerdaPeso = 1,
        [Display(Name = "Ganho de Massa")] GanhoMassa = 2,
        [Display(Name = "Resistência")] Resistencia = 3,
        [Display(Name = "Reabilitação")] Reabilitacao = 4,
        [Display(Name = "Manutenção")] Manutencao = 5
    }
}