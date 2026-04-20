using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Models.Entities
{
    /// <summary>
    /// Aula recorrente semanal num ginásio.
    /// </summary>
    public class Aula
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da aula é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Nome da Aula")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O dia da semana é obrigatório")]
        [Display(Name = "Dia da Semana")]
        public DiaSemana DiaSemana { get; set; }

        [Required(ErrorMessage = "A hora é obrigatória")]
        [Display(Name = "Hora")]
        public TimeOnly Hora { get; set; }

        [Required]
        [Range(15, 180, ErrorMessage = "Duração entre 15 e 180 minutos")]
        [Display(Name = "Duração (minutos)")]
        public int DuracaoMinutos { get; set; }

        [Required(ErrorMessage = "O plano de treino é obrigatório")]
        [Display(Name = "Plano de Treino")]
        public int PlanoTreinoId { get; set; }
        public virtual PlanoTreino? PlanoTreino { get; set; }

        [Required(ErrorMessage = "O instrutor é obrigatório")]
        [Display(Name = "Instrutor")]
        public int InstrutorId { get; set; }
        public virtual Instrutor? Instrutor { get; set; }

        [Required(ErrorMessage = "A sala é obrigatória")]
        [Display(Name = "Sala")]
        public int SalaId { get; set; }
        public virtual Sala? Sala { get; set; }

        [Display(Name = "Ativa")]
        public bool Ativa { get; set; } = true;

        public virtual ICollection<InscricaoAula> Inscricoes { get; set; } = new List<InscricaoAula>();

        [NotMapped]
        public int VagasDisponiveis => (Sala?.CapacidadeMaxima ?? 0) - Inscricoes.Count;

        [NotMapped]
        public bool Lotada => VagasDisponiveis <= 0;

        [NotMapped]
        public string HoraFim => Hora.AddMinutes(DuracaoMinutos).ToString("HH:mm");
    }

    public enum DiaSemana
    {
        [Display(Name = "Segunda-feira")]  Segunda  = 1,
        [Display(Name = "Terça-feira")]    Terca    = 2,
        [Display(Name = "Quarta-feira")]   Quarta   = 3,
        [Display(Name = "Quinta-feira")]   Quinta   = 4,
        [Display(Name = "Sexta-feira")]    Sexta    = 5,
        [Display(Name = "Sábado")]         Sabado   = 6,
        [Display(Name = "Domingo")]        Domingo  = 7
    }
}
