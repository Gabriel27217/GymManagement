using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Models.Entities
{
    /// <summary>
    /// Representa uma aula de grupo recorrente (ex: Yoga, Pilates, Spinning).
    /// Esta entidade gere o horário, a localização (sala) e o instrutor responsável.
    /// </summary>
    public class Aula
    {
        /// <summary>
        /// Identificador único da aula (Chave Primária).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome da modalidade ou aula específica.
        /// </summary>
        [Required(ErrorMessage = "O nome da aula é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Nome da Aula")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Categoria ou tipo de atividade física à qual a aula pertence.
        /// </summary>
        [Required(ErrorMessage = "A categoria é obrigatória")]
        [Display(Name = "Categoria")]
        public CategoriaAula Categoria { get; set; } = CategoriaAula.Outro;

        /// <summary>
        /// Dia da semana em que a aula ocorre recorrentemente.
        /// </summary>
        [Required(ErrorMessage = "O dia da semana é obrigatório")]
        [Display(Name = "Dia da Semana")]
        public DiaSemana DiaSemana { get; set; }

        /// <summary>
        /// Hora de início da aula.
        /// </summary>
        [Required(ErrorMessage = "A hora é obrigatória")]
        [Display(Name = "Hora")]
        public TimeOnly Hora { get; set; }

        /// <summary>
        /// Tempo total da aula expresso em minutos.
        /// </summary>
        [Required]
        [Range(15, 180, ErrorMessage = "Duração entre 15 e 180 minutos")]
        [Display(Name = "Duração (minutos)")]
        public int DuracaoMinutos { get; set; }

        /// <summary>
        /// ID do instrutor que leciona a aula.
        /// </summary>
        [Required(ErrorMessage = "O instrutor é obrigatório")]
        [Display(Name = "Instrutor")]
        public int InstrutorId { get; set; }

        /// <summary>
        /// Objeto de navegação para os detalhes do instrutor.
        /// </summary>
        public virtual Instrutor? Instrutor { get; set; }

        /// <summary>
        /// ID da sala onde a aula é realizada.
        /// </summary>
        [Required(ErrorMessage = "A sala é obrigatória")]
        [Display(Name = "Sala")]
        public int SalaId { get; set; }

        /// <summary>
        /// Objeto de navegação para os detalhes da sala.
        /// </summary>
        public virtual Sala? Sala { get; set; }

        /// <summary>
        /// Indica se a aula está disponível no horário atual do ginásio.
        /// </summary>
        [Display(Name = "Ativa")]
        public bool Ativa { get; set; } = true;

        /// <summary>
        /// Lista de inscrições de alunos para esta aula específica.
        /// </summary>
        public virtual ICollection<InscricaoAula> Inscricoes { get; set; } = new List<InscricaoAula>();

        /// <summary>
        /// Calcula o número de lugares ainda disponíveis com base na capacidade da sala.
        /// </summary>
        [NotMapped]
        public int VagasDisponiveis => (Sala?.CapacidadeMaxima ?? 0) - Inscricoes.Count;

        /// <summary>
        /// Verifica se a aula atingiu o limite máximo de participantes.
        /// </summary>
        [NotMapped]
        public bool Lotada => VagasDisponiveis <= 0;

        /// <summary>
        /// Formata a hora de término da aula somando a duração à hora de início.
        /// </summary>
        [NotMapped]
        public string HoraFim => Hora.AddMinutes(DuracaoMinutos).ToString("HH:mm");
    }

    /// <summary>
    /// Tipos de modalidades desportivas disponíveis no ginásio.
    /// </summary>
    public enum CategoriaAula
    {
        [Display(Name = "Yoga")] Yoga = 1,
        [Display(Name = "Pilates")] Pilates = 2,
        [Display(Name = "Spinning")] Spinning = 3,
        [Display(Name = "Musculação")] Musculacao = 4,
        [Display(Name = "Zumba")] Zumba = 5,
        [Display(Name = "Boxe")] Boxe = 6,
        [Display(Name = "Natação")] Natacao = 7,
        [Display(Name = "Crossfit")] Crossfit = 8,
        [Display(Name = "Outro")] Outro = 99
    }

    /// <summary>
    /// Dias úteis e fim de semana para agendamento de aulas.
    /// </summary>
    public enum DiaSemana
    {
        [Display(Name = "Segunda-feira")] Segunda = 1,
        [Display(Name = "Terça-feira")] Terca = 2,
        [Display(Name = "Quarta-feira")] Quarta = 3,
        [Display(Name = "Quinta-feira")] Quinta = 4,
        [Display(Name = "Sexta-feira")] Sexta = 5,
        [Display(Name = "Sábado")] Sabado = 6,
        [Display(Name = "Domingo")] Domingo = 7
    }
}