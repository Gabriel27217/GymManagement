using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models.Entities
{
    /// <summary>
    /// Representa um utilizador do sistema (Administrador ou Cliente).
    /// Entidade principal para a gestão de acessos e perfis no sistema do ginásio.
    /// Trabalho académico - Desenvolvimento Web 2024/2025
    /// Autores: 27217, 24634
    /// </summary>
    public class Utilizador
    {
        /// <summary>
        /// Identificador único do utilizador (Chave Primária).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome completo do utilizador.
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Endereço de correio eletrónico único para autenticação e contacto.
        /// </summary>
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Hash da palavra-passe para garantir a segurança dos dados de acesso.
        /// </summary>
        [Required(ErrorMessage = "A password é obrigatória")]
        [StringLength(256)]
        [Display(Name = "Password")]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Define o nível de privilégio do utilizador no sistema.
        /// </summary>
        [Required]
        [Display(Name = "Tipo de Utilizador")]
        public TipoUtilizador Role { get; set; } = TipoUtilizador.Cliente;

        /// <summary>
        /// Data e hora em que a conta foi criada no sistema.
        /// </summary>
        [Display(Name = "Data de Registo")]
        public DateTime DataRegisto { get; set; } = DateTime.Now;

        /// <summary>
        /// Indica se a conta do utilizador se encontra ativa ou suspensa.
        /// </summary>
        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        /// <summary>
        /// Número de contacto telefónico (opcional).
        /// </summary>
        [Phone(ErrorMessage = "Número de telefone inválido")]
        [StringLength(20)]
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        /// <summary>
        /// Coleção de inscrições em aulas associadas ao utilizador.
        /// Representa a relação muitos-para-muitos com a entidade Aula.
        /// </summary>
        public virtual ICollection<InscricaoAula> Inscricoes { get; set; } = new List<InscricaoAula>();

        /// <summary>
        /// Histórico de presenças/frequências registadas pelo utilizador.
        /// Representa a relação um-para-muitos com a entidade Frequencia.
        /// </summary>
        public virtual ICollection<Frequencia> Frequencias { get; set; } = new List<Frequencia>();
    }

    /// <summary>
    /// Enumeração que define os perfis de acesso disponíveis no sistema.
    /// </summary>
    public enum TipoUtilizador
    {
        /// <summary>
        /// Perfil com privilégios totais de gestão.
        /// </summary>
        [Display(Name = "Administrador")]
        Admin = 0,

        /// <summary>
        /// Perfil de cliente com acesso limitado às suas marcações e dados.
        /// </summary>
        [Display(Name = "Cliente")]
        Cliente = 1
    }
}