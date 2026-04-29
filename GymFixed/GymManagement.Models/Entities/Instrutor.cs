using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models.Entities
{
    /// <summary>
    /// Instrutor que leciona aulas no ginásio.
    /// </summary>
    public class Instrutor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Especialidade")]
        public string? Especialidade { get; set; }

        [Phone]
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        // Um instrutor leciona várias aulas
        public virtual ICollection<Aula> Aulas { get; set; } = new List<Aula>();
    }
}
