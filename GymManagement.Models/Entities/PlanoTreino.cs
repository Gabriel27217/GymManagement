using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models.Entities
{
    /// <summary>
    /// Plano de treino que agrupa um tipo de aulas.
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

        [Display(Name = "Nível de Dificuldade")]
        public NivelDificuldade Nivel { get; set; } = NivelDificuldade.Iniciante;

        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        // Um plano tem várias aulas (muitos-para-um do lado da Aula)
        public virtual ICollection<Aula> Aulas { get; set; } = new List<Aula>();
    }

    public enum NivelDificuldade
    {
        [Display(Name = "Iniciante")]
        Iniciante = 0,

        [Display(Name = "Intermédio")]
        Intermedio = 1,

        [Display(Name = "Avançado")]
        Avancado = 2
    }
}
