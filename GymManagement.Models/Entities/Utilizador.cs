using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models.Entities
{
    /// <summary>
    /// Utilizador do sistema (Administrador ou Cliente).
    /// Trabalho académico - Desenvolvimento Web 2024/2025
    /// Autores: 27217, 24634
    /// </summary>
    public class Utilizador
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A password é obrigatória")]
        [StringLength(256)]
        [Display(Name = "Password")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tipo de Utilizador")]
        public TipoUtilizador Role { get; set; } = TipoUtilizador.Cliente;

        [Display(Name = "Data de Registo")]
        public DateTime DataRegisto { get; set; } = DateTime.Now;

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        [Phone(ErrorMessage = "Número de telefone inválido")]
        [StringLength(20)]
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        // Relação muitos-para-muitos com Aula
        public virtual ICollection<InscricaoAula> Inscricoes { get; set; } = new List<InscricaoAula>();

        // Relação um-para-muitos com Frequencia
        public virtual ICollection<Frequencia> Frequencias { get; set; } = new List<Frequencia>();
    }

    public enum TipoUtilizador
    {
        [Display(Name = "Administrador")]
        Admin = 0,

        [Display(Name = "Cliente")]
        Cliente = 1
    }
}
