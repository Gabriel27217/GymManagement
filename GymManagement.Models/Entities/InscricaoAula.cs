using System.ComponentModel.DataAnnotations;

namespace GymManagement.Models.Entities
{
    /// <summary>
    /// Tabela de junção muitos-para-muitos entre Utilizador e Aula.
    /// </summary>
    public class InscricaoAula
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Data de Inscrição")]
        public DateTime DataInscricao { get; set; } = DateTime.Now;

        [Required]
        public int UtilizadorId { get; set; }
        public virtual Utilizador? Utilizador { get; set; }

        [Required]
        public int AulaId { get; set; }
        public virtual Aula? Aula { get; set; }
    }

    /// <summary>
    /// Registo de presença livre no ginásio (sem aula associada).
    /// </summary>
    public class Frequencia
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Entrada")]
        public DateTime Entrada { get; set; } = DateTime.Now;

        [Display(Name = "Saída")]
        public DateTime? Saida { get; set; }

        [StringLength(200)]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        [Required]
        public int UtilizadorId { get; set; }
        public virtual Utilizador? Utilizador { get; set; }

        // Propriedades calculadas — não mapeadas para BD
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public TimeSpan? Duracao => Saida.HasValue ? Saida - Entrada : null;

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool EmGinasio => !Saida.HasValue;
    }
}
