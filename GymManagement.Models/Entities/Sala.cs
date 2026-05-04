using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models.Entities
{
    /// <summary>
    /// Representa uma sala física nas instalações do ginásio onde decorrem as aulas.
    /// Contém informações sobre a lotação e disponibilidade do espaço.
    /// </summary>
    public class Sala
    {
        /// <summary>
        /// Identificador único da sala (Chave Primária).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome ou designação da sala (Ex: Sala de Musculação, Estúdio 1).
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Número máximo de alunos que a sala pode comportar simultaneamente.
        /// </summary>
        [Required(ErrorMessage = "A capacidade é obrigatória")]
        [Range(1, 500, ErrorMessage = "A capacidade deve estar entre 1 e 500")]
        [Display(Name = "Capacidade Máxima")]
        public int CapacidadeMaxima { get; set; }

        /// <summary>
        /// Breve descrição das características da sala ou equipamentos disponíveis.
        /// </summary>
        [StringLength(300)]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        /// <summary>
        /// Indica se a sala está disponível para agendamento de novas aulas.
        /// </summary>
        [Display(Name = "Ativa")]
        public bool Ativa { get; set; } = true;

        /// <summary>
        /// Coleção de aulas agendadas para esta sala.
        /// Estabelece a relação um-para-muitos com a entidade Aula.
        /// </summary>
        public virtual ICollection<Aula> Aulas { get; set; } = new List<Aula>();
    }
}