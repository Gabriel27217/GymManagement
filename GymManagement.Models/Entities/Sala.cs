using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models.Entities
{
    /// <summary>
    /// Sala onde decorrem as aulas do ginásio.
    /// </summary>
    public class Sala
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A capacidade é obrigatória")]
        [Range(1, 500, ErrorMessage = "A capacidade deve estar entre 1 e 500")]
        [Display(Name = "Capacidade Máxima")]
        public int CapacidadeMaxima { get; set; }

        [StringLength(300)]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        [Display(Name = "Ativa")]
        public bool Ativa { get; set; } = true;

        // Uma sala aloja várias aulas
        public virtual ICollection<Aula> Aulas { get; set; } = new List<Aula>();
    }
}
