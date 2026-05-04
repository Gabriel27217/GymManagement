using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models.Entities
{
    /// <summary>
    /// Representa um instrutor (colaborador técnico) que leciona aulas no ginásio.
    /// Armazena informações de contacto, especialidade e o histórico de aulas atribuídas.
    /// </summary>
    public class Instrutor
    {
        /// <summary>
        /// Identificador único do instrutor (Chave Primária).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome completo do instrutor.
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Endereço de correio eletrónico profissional do instrutor.
        /// </summary>
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Área de foco principal (ex: Pilates, Crossfit, Musculação).
        /// </summary>
        [StringLength(100)]
        [Display(Name = "Especialidade")]
        public string? Especialidade { get; set; }

        /// <summary>
        /// Número de contacto telefónico do instrutor.
        /// </summary>
        [Phone]
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        /// <summary>
        /// Indica se o instrutor faz parte do quadro atual de colaboradores ativos.
        /// </summary>
        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        /// <summary>
        /// Coleção de aulas que estão sob a responsabilidade deste instrutor.
        /// Estabelece a relação um-para-muitos com a entidade Aula.
        /// </summary>
        public virtual ICollection<Aula> Aulas { get; set; } = new List<Aula>();
    }
}